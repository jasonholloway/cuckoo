using Cuckoo;
using Cuckoo.Fody.Cecil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    using Mono.Collections.Generic;
    using Refl = System.Reflection;

    internal partial class MethodWeaver
    {
        WeaveSpec _spec;
        Action<string> _logger;

        public MethodWeaver(WeaveSpec spec, Action<string> logger) 
        {
            _spec = spec;
            _logger = logger;
        }


        WeaveContext CreateContext(WeaveSpec spec) {
            var tCont = spec.Method.DeclaringType;

            var tContRef = tCont.HasGenericParameters
                            ? tCont.MakeGenericInstanceType(tCont.GenericParameters.ToArray())
                            : (TypeReference)tCont;

            var module = tCont.Module;

            return new WeaveContext() {
                tCont = tCont,
                tContRef = tContRef,
                Module = module,
                mOuter = spec.Method,
                NameSource = new NameSource(tCont),
                RefMap = new RefMap(module, spec.Method),
                Logger = _logger
            };
        }







        public void Weave() 
        {   
            var ctx = CreateContext(_spec);

            var fRoostRef = CreateRoost(ctx);

            var mInner = TransplantOuterToInner(ctx);

            var mOuter = WeaveOuterMethod(ctx, _spec.Cuckoos, mInner);

            AddCuckooedAttribute(ctx, mOuter, mInner);

            ctx.Logger(string.Format("Mod applied to {0}!", ctx.mOuter.FullName));
        }



        void AddCuckooedAttribute(WeaveContext ctx, MethodDefinition mOuter, MethodDefinition mInner) {
            var R = ctx.RefMap;
            var mod = ctx.Module;

            var atUsurped = new CustomAttribute(R.CuckooedAtt_mCtor);
            atUsurped.ConstructorArguments.Add(
                            new CustomAttributeArgument(mod.TypeSystem.String, mInner.Name));

            mOuter.CustomAttributes.Add(atUsurped);
        }



        MethodDefinition TransplantOuterToInner(WeaveContext ctx) {
            var tCont = ctx.tCont;
            var mOuter = ctx.mOuter;
            var names = ctx.NameSource;

            var mInner = mOuter.CloneTo(
                                    tCont,
                                    m => {
                                        m.Name = names.GetElementName("CUCKOOED", mOuter.Name);

                                        m.Attributes ^= MethodAttributes.Public | MethodAttributes.Private;

                                        if(mOuter.IsConstructor) {
                                            m.Attributes &= ~MethodAttributes.SpecialName & ~MethodAttributes.RTSpecialName;

                                            GetInitialCtorInstructions(m.Body).ToList()
                                                .ForEach(i => m.Body.Instructions.Remove(i));
                                        }
                                    });

            ctx.mInner = mInner;

            return mInner;
        }

        Instruction[] GetInitialCtorInstructions(MethodBody methodBody) {
            Instruction last = null;

            return methodBody.Instructions
                                .TakeWhile(i => {
                                    if(last != null
                                        && last.OpCode.Code == Code.Call
                                        && last.Operand is MethodReference
                                        && ((MethodReference)last.Operand).Resolve().IsConstructor) 
                                    {
                                        return false;
                                    }

                                    last = i;
                                    return true;
                                })
                                .ToArray();
        }






        FieldReference CreateRoost(WeaveContext ctx) {
            var mod = ctx.Module;
            var R = ctx.RefMap;
            var mOuter = ctx.mOuter;
            var mInner = ctx.mInner;
            var names = ctx.NameSource;
            var tCont = ctx.tCont;

            var tContRef = tCont.HasGenericParameters
                            ? tCont.MakeGenericInstanceType(tCont.GenericParameters.ToArray())
                            : (TypeReference)tCont;

            ctx.tContRef = tContRef;

            /////////////////////////////////////////////////////////////////////////////////////////////
            //Create static CallSite ///////////////////////////////////////////////////////////////////
            string callSiteName = names.GetElementName("ROOST", mOuter.Name);

            var fRoost = tCont.AddField(
                                    R.Roost_Type,
                                    callSiteName);

            fRoost.Attributes = FieldAttributes.Private
                                        | FieldAttributes.Static
                                        | FieldAttributes.InitOnly;

            ctx.fRoost = fRoost;

            var fRoostRef = fRoost.CloneWithNewDeclaringType(tContRef);


            tCont.AppendToStaticCtor(
                (i, m) => {
                    //var vMethodBase = m.Body.AddVariable<Refl.MethodBase>();
                    var vMethod = m.Body.AddVariable<Method>();
                    var vCuckoo = m.Body.AddVariable<ICuckoo>();
                    var vCuckoos = m.Body.AddVariable<ICuckoo[]>();
                    
                    //i.Emit(OpCodes.Ldtoken, mOuter);
                    //i.Emit(OpCodes.Ldtoken, tContRef);
                    //i.Emit(OpCodes.Call, R.MethodBase_mGetMethodFromHandle);
                    ////i.Emit(OpCodes.Castclass, R.MethodInfo_Type);
                    //i.Emit(OpCodes.Stloc, vMethodBase);

                    i.Emit(OpCodes.Ldnull); //CONSTRUCT METHOD HERE!!!!!!!
                    i.Emit(OpCodes.Stloc_S, vMethod);
                    
                    /////////////////////////////////////////////////////////////////////
                    //Load ICuckoo instances into array ////////////////////////////////
                    i.Emit(OpCodes.Ldc_I4, _spec.Cuckoos.Length);
                    i.Emit(OpCodes.Newarr, R.ICuckoo_Type);
                    i.Emit(OpCodes.Stloc_S, vCuckoos);
                    
                    foreach(var cuckoo in _spec.Cuckoos) {
                        var att = cuckoo.Attribute;
                        var tAtt = att.AttributeType;

                        if(att.HasConstructorArguments) {
                            var mCtor = tAtt.ReferenceMethod(c => m.IsConstructor
                                                                    && c.Parameters.Select(p => p.ParameterType)
                                                                        .SequenceEqual(
                                                                            att.ConstructorArguments.Select(a => a.Type) 
                                                                        ));

                            foreach(var ctorArg in att.ConstructorArguments) {
                                i.EmitConstant(ctorArg.Type, ctorArg.Value);
                            }

                            i.Emit(OpCodes.Newobj, mCtor);
                        }
                        else {
                            var mCtor = tAtt.ReferenceMethod(c => c.IsConstructor
                                                                    && !c.HasParameters );
                            i.Emit(OpCodes.Newobj, mCtor);
                        }

                        i.Emit(OpCodes.Stloc, vCuckoo);


                        if(att.HasFields) {
                            foreach(var namedCtorArg in att.Fields) {
                                var field = tAtt.ReferenceField(namedCtorArg.Name);

                                i.Emit(OpCodes.Ldloc, vCuckoo);
                                i.Emit(OpCodes.Castclass, tAtt);
                                i.EmitConstant(namedCtorArg.Argument.Type, namedCtorArg.Argument.Value);
                                i.Emit(OpCodes.Stfld, field);
                            }
                        }

                        if(att.HasProperties) {
                            foreach(var propArg in att.Properties) {
                                var mSet = tAtt.ReferencePropertySetter(propArg.Name);

                                i.Emit(OpCodes.Ldloc, vCuckoo);
                                i.Emit(OpCodes.Castclass, tAtt);
                                i.EmitConstant(propArg.Argument.Type, propArg.Argument.Value);
                                i.Emit(OpCodes.Call, mSet);
                            }
                        }

                        i.Emit(OpCodes.Ldloc_S, vCuckoos);
                        i.Emit(OpCodes.Ldc_I4, cuckoo.Index);
                        i.Emit(OpCodes.Ldloc_S, vCuckoo);
                        i.Emit(OpCodes.Stelem_Ref);
                    }
                    
                    ////////////////////////////////////////////////////////////////////
                    //Construct and emplace Roost
                    i.Emit(OpCodes.Ldloc, vMethod);
                    i.Emit(OpCodes.Ldloc, vCuckoos);
                    i.Emit(OpCodes.Newobj, R.Roost_mCtor);
                    i.Emit(OpCodes.Stsfld, fRoostRef);


                    ////////////////////////////////////////////////////////////////////
                    //Init Cuckoos ////////////////////////////////////////////////////
                    foreach(var cuckoo in _spec.Cuckoos) {
                        i.Emit(OpCodes.Ldloc, vCuckoos);
                        i.Emit(OpCodes.Ldc_I4, cuckoo.Index);
                        i.Emit(OpCodes.Ldelem_Ref);

                        i.Emit(OpCodes.Ldsfld, fRoostRef);
                        i.Emit(OpCodes.Callvirt, R.ICuckoo_mOnRoost);
                    }
                });

            return fRoostRef;
        }






        MethodDefinition WeaveOuterMethod(
            WeaveContext ctx, 
            IEnumerable<CuckooSpec> cuckoos, 
            MethodDefinition mInner) 
        {
            var R = ctx.RefMap;
            var mOuter = ctx.mOuter;
            var fRoost = ctx.fRoost;
            var tCont = ctx.tCont;
            var tContRef = ctx.tContRef;

            var fRoostRef = tCont.HasGenericParameters
                                ? fRoost.CloneWithNewDeclaringType(tContRef)
                                : fRoost;

            var mOuterRef = mOuter.HasGenericParameters
                            ? mOuter.MakeGenericInstanceMethod(mOuter.GenericParameters.ToArray())
                            : (MethodReference)mOuter;


            var contGenArgs = tContRef is GenericInstanceType
                                ? ((GenericInstanceType)tContRef).GenericArguments.ToArray()
                                : new TypeReference[0];

            var methodGenArgs = mOuterRef is GenericInstanceMethod
                                ? ((GenericInstanceMethod)mOuterRef).GenericArguments.ToArray()
                                : new TypeReference[0];


            var args = ArgSpec.CreateAll(ctx, mOuterRef.Parameters);


            var callWeaver = new CallWeaver(ctx);

            var call = callWeaver.Weave(mOuterRef, args);
            
            if(call.RequiresInstanciation) {
                call = call.Instanciate(contGenArgs, methodGenArgs);
            }


            var initialCtorInstructions = mOuter.IsConstructor
                                            ? GetInitialCtorInstructions(mOuter.Body)
                                            : null;

            mOuter.Body = new MethodBody(mOuter);

            mOuter.Compose(
                (i, m) => {
                    var vCall = m.Body.AddVariable(call.Type);
                    
                    i.Emit(OpCodes.Ldsfld, fRoostRef);

                    if(!m.IsStatic) {
                        i.Emit(OpCodes.Ldarg_0);
                    }
                    
                    i.Emit(OpCodes.Ldc_I4, args.Length);
                    i.Emit(OpCodes.Newarr, R.ICallArg_Type);

                    foreach(var arg in args) {
                        i.Emit(OpCodes.Dup);
                        i.Emit(OpCodes.Ldc_I4, arg.Index);

                        i.Emit(OpCodes.Ldnull); //LOAD PARAMETER HERE!!!!

                        if(arg.IsByRef) {
                            i.Emit(OpCodes.Ldarg_S, arg.Param);
                            i.Emit(OpCodes.Ldobj, arg.Type);
                        }
                        else {
                            i.Emit(OpCodes.Ldarg_S, arg.Param);
                        }

                        i.Emit(OpCodes.Call, arg.CallArg_mCtor);

                        i.Emit(OpCodes.Stelem_Ref);
                    }

                    i.Emit(OpCodes.Newobj, call.CtorMethod);
                    i.Emit(OpCodes.Stloc_S, vCall);


                    i.Emit(OpCodes.Ldloc_S, vCall);
                    i.Emit(OpCodes.Call, call.PreDispatchMethod);


                    if(initialCtorInstructions != null) {
                        foreach(var inst in initialCtorInstructions) {
                            i.Append(inst);
                        }
                    }


                    i.Emit(OpCodes.Ldloc_S, vCall);
                    i.Emit(OpCodes.Call, call.DispatchMethod);


                    foreach(var callArg in call.Args.Where(a => a.IsByRef)) {
                        i.Emit(OpCodes.Ldarg_S, callArg.MethodArg.Param);
                        i.Emit(OpCodes.Ldloc_S, vCall);
                        i.Emit(OpCodes.Ldfld, call.ArgsField);
                        i.Emit(OpCodes.Ldc_I4, callArg.Index);
                        i.Emit(OpCodes.Ldelem_Ref);
                        i.Emit(OpCodes.Ldfld, callArg.CallArg_fValue);
                        i.Emit(OpCodes.Stobj, callArg.Type);
                    }

                    if(call.ReturnsValue) {
                        i.Emit(OpCodes.Ldloc, vCall);
                        i.Emit(OpCodes.Ldfld, call.ReturnField);
                    }

                    i.Emit(OpCodes.Ret);
                });

            return mOuter;
        }





        /*
        CallInfo WeaveCallHierarchy(
            WeaveContext ctx, 
            IEnumerable<CuckooSpec> cuckoos, 
            MethodDefinition mInner,
            MethodReference mOuterRef) 
        {
            CallWeaver callWeaver;

            var cuckoo = cuckoos.First();

            if(cuckoos.Skip(1).Any()) {
                var nextCall = WeaveCallHierarchy(ctx, cuckoos.Skip(1), mInner, mOuterRef);

                callWeaver = new MediateCallWeaver(ctx, cuckoo, nextCall);
            }
            else {
                callWeaver = new FinalCallWeaver(ctx, cuckoo, mInner);
            }

            return callWeaver.Weave(mOuterRef);
        }
        */




        //MethodDefinition UsurpOuterMethod(WeaveContext ctx, CallWeaver topCallWeaver) {
        //    var mOuter = ctx.mOuter;
        //    var R = ctx.RefMap;
        //    var fRoost = ctx.fRoost;
        //    var tCont = ctx.tCont;
        //    var tContRef = ctx.tContRef;

        //    var fRoostRef = tCont.HasGenericParameters
        //                        ? fRoost.CloneWithNewDeclaringType(tContRef)
        //                        : fRoost;

        //    var mOuterRef = mOuter.HasGenericParameters
        //                    ? mOuter.MakeGenericInstanceMethod(mOuter.GenericParameters.ToArray())
        //                    : (MethodReference)mOuter;


        //    var contGenArgs = tContRef is GenericInstanceType
        //                        ? ((GenericInstanceType)tContRef).GenericArguments.ToArray()
        //                        : new TypeReference[0];

        //    var methodGenArgs = mOuterRef is GenericInstanceMethod
        //                        ? ((GenericInstanceMethod)mOuterRef).GenericArguments.ToArray()
        //                        : new TypeReference[0];


        //    var call = topCallWeaver.Weave(mOuterRef);

        //    if(call.RequiresInstanciation) {
        //        call = call.Instanciate(contGenArgs.Concat(methodGenArgs));
        //    }
            
        //    var initialCtorInstructions = mOuter.IsConstructor
        //                                    ? GetInitialCtorInstructions(mOuter.Body)
        //                                    : null;


        //    mOuter.Body = new MethodBody(mOuter);

        //    mOuter.Compose(
        //        (i, m) => {
        //            var vCall = m.Body.AddVariable(call.Type);
        //            var vCuckoo = m.Body.AddVariable<ICuckoo>();

        //            i.Emit(OpCodes.Ldsfld, fRoostRef);

        //            if(!m.IsStatic) {
        //                i.Emit(OpCodes.Ldarg_0);
        //            }

        //            foreach(var param in m.Parameters) {
        //                if(param.ParameterType.IsByReference) {
        //                    i.Emit(OpCodes.Ldarg_S, param);
        //                    i.Emit(OpCodes.Ldobj, param.ParameterType.GetElementType());
        //                }
        //                else {
        //                    i.Emit(OpCodes.Ldarg_S, param);
        //                }
        //            }

        //            i.Emit(OpCodes.Newobj, call.CtorMethod);
        //            i.Emit(OpCodes.Stloc_S, vCall);

        //            i.Emit(OpCodes.Ldsfld, fRoostRef);
        //            i.Emit(OpCodes.Call, R.Roost_mGetUsurpers);
        //            i.Emit(OpCodes.Ldc_I4_0);
        //            i.Emit(OpCodes.Ldelem_Ref);
        //            i.Emit(OpCodes.Stloc_S, vCuckoo);

        //            //Init call with 


        //            i.Emit(OpCodes.Ldloc_S, vCuckoo);
        //            i.Emit(OpCodes.Ldloc_S, vCall);
        //            i.Emit(OpCodes.Callvirt, R.ICuckoo_mOnBeforeCall);
                    
        //            //interpose ctor init stuff here
        //            //...

        //            i.Emit(OpCodes.Ldloc_S, vCuckoo);
        //            i.Emit(OpCodes.Ldloc_S, vCall);
        //            i.Emit(OpCodes.Callvirt, R.ICuckoo_mOnCall);

        //            i.Emit(OpCodes.Ldloc, vCall);
        //            i.Emit(OpCodes.Call, call.AfterUsurpMethod);

        //            foreach(var arg in call.Args.Where(a => a.IsByRef)) {
        //                i.Emit(OpCodes.Ldarg_S, arg.MethodParam);
        //                i.Emit(OpCodes.Ldloc_S, vCall);
        //                i.Emit(OpCodes.Ldfld, arg.Field);
        //                i.Emit(OpCodes.Stobj, arg.Field.FieldType);
        //            }

        //            if(call.ReturnsValue) {
        //                i.Emit(OpCodes.Ldloc, vCall);
        //                i.Emit(OpCodes.Ldfld, call.ReturnField);
        //            }

        //            i.Emit(OpCodes.Ret);
        //        });


        //    if(initialCtorInstructions != null) {
        //        foreach(var inst in initialCtorInstructions.Reverse()) {
        //            mOuter.Body.Instructions.Insert(0, inst);
        //        }
        //    }

        //    return mOuter;
        //}




    }
}
