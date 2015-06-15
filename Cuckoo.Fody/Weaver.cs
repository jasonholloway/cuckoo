using Cuckoo.Common;
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
    using Refl = System.Reflection;

    internal class Weaver
    {
        WeaveSpec _spec;
        Action<string> _logger;

        public Weaver(WeaveSpec spec, Action<string> logger) 
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

        
        public void Apply() 
        {
            var ctx = CreateContext(_spec);

            var fRoostRef = CreateRoost(ctx);
            
            var mInner = TransplantOuterToInner(ctx);

            var callWeaver = (CallWeaver)new FinalCallWeaver(ctx, mInner);

            for(int iU = 1; iU < _spec.CuckooAttributes.Length; iU++) {
                callWeaver = new MediateCallWeaver(ctx, callWeaver, iU);
            }

            UsurpOuterMethod(ctx, callWeaver);

            AddCuckooedAttribute(ctx);

            ctx.Logger(string.Format("Mod applied to {0}!", ctx.mOuter.FullName));
        }



        void AddCuckooedAttribute(WeaveContext ctx) {
            var R = ctx.RefMap;
            var mod = ctx.Module;
            var mInner = ctx.mInner;
            var mOuter = ctx.mOuter;

            var atUsurped = new CustomAttribute(R.CuckooedAtt_mCtor);
            atUsurped.ConstructorArguments.Add(
                            new CustomAttributeArgument(mod.TypeSystem.String, mInner.Name));

            mOuter.CustomAttributes.Add(atUsurped);
        }



        MethodDefinition TransplantOuterToInner(WeaveContext ctx) {
            var mOuter = ctx.mOuter;
            var names = ctx.NameSource;

            string innerName = names.GetElementName("CUCKOOED", mOuter.Name);

            var mInner = mOuter.CopyToNewSibling(innerName);

            mInner.Attributes = MethodAttributes.Private 
                                | MethodAttributes.HideBySig 
                                | MethodAttributes.SpecialName;

            ctx.mInner = mInner;

            return mInner;
            
            //var mInnerRef = mInner.CloneWithNewDeclaringType(ctx.tContRef);
            //return mInnerRef;
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
                                    R.Roost_TypeRef,
                                    callSiteName);

            fRoost.Attributes = FieldAttributes.Private
                                        | FieldAttributes.Static
                                        | FieldAttributes.InitOnly;

            ctx.fRoost = fRoost;

            var fRoostRef = fRoost.CloneWithNewDeclaringType(tContRef);


            tCont.AppendToStaticCtor(
                (i, m) => {
                    var vMethodInfo = m.Body.AddVariable(R.MethodInfo_TypeRef);
                    var vCuckoo = m.Body.AddVariable<ICuckoo>();
                    var vCuckoos = m.Body.AddVariable<ICuckoo[]>();
                    
                    i.Emit(OpCodes.Ldtoken, mOuter);
                    i.Emit(OpCodes.Ldtoken, tContRef);
                    i.Emit(OpCodes.Call, R.MethodInfo_mGetMethodFromHandle);
                    i.Emit(OpCodes.Castclass, R.MethodInfo_TypeRef);
                    i.Emit(OpCodes.Stloc, vMethodInfo);
                    
                    /////////////////////////////////////////////////////////////////////
                    //Load usurper instances into array ////////////////////////////////
                    i.Emit(OpCodes.Ldc_I4, _spec.CuckooAttributes.Length);
                    i.Emit(OpCodes.Newarr, R.ICuckoo_TypeRef);
                    i.Emit(OpCodes.Stloc_S, vCuckoos);
                    
                    int iA = 0;

                    foreach(var att in _spec.CuckooAttributes) {
                        var tAtt = mod.ImportReference(att.AttributeType);

                        if(att.HasConstructorArguments) {
                            var mCtor = mod.ImportReference(
                                                    tAtt.Resolve()
                                                        .GetConstructors()
                                                        .First(c => c.Parameters
                                                                        .Select(p => p.ParameterType)
                                                                        .SequenceEqual(
                                                                            att.ConstructorArguments
                                                                                .Select(a => a.Type)
                                                                            )
                                                        )
                                                    );

                            foreach(var ctorArg in att.ConstructorArguments) {
                                i.EmitConstant(ctorArg.Type, ctorArg.Value);
                            }

                            i.Emit(OpCodes.Newobj, mCtor);
                        }
                        else {
                            var mCtor = mod.ImportReference(
                                                    tAtt.Resolve()
                                                        .GetConstructors()
                                                        .First(c => !c.HasParameters)
                                                    );

                            i.Emit(OpCodes.Newobj, mCtor);
                        }

                        i.Emit(OpCodes.Stloc, vCuckoo);


                        if(att.HasFields) {
                            foreach(var namedCtorArg in att.Fields) {
                                var field = tAtt.Resolve().GetField(namedCtorArg.Name);

                                i.Emit(OpCodes.Ldloc, vCuckoo);
                                i.Emit(OpCodes.Castclass, tAtt);
                                i.EmitConstant(namedCtorArg.Argument.Type, namedCtorArg.Argument.Value);
                                i.Emit(OpCodes.Stfld, field);
                            }
                        }

                        if(att.HasProperties) {
                            foreach(var propCtorArg in att.Properties) {
                                var prop = tAtt.Resolve().Properties
                                                .Where(p => p.SetMethod != null)
                                                .First(p => p.Name == propCtorArg.Name);

                                i.Emit(OpCodes.Ldloc, vCuckoo);
                                i.Emit(OpCodes.Castclass, tAtt);
                                i.EmitConstant(propCtorArg.Argument.Type, propCtorArg.Argument.Value);
                                i.Emit(OpCodes.Call, prop.SetMethod);
                            }
                        }

                        i.Emit(OpCodes.Ldloc_S, vCuckoos);
                        i.Emit(OpCodes.Ldc_I4, iA);
                        i.Emit(OpCodes.Ldloc_S, vCuckoo);
                        i.Emit(OpCodes.Stelem_Ref);

                        iA++;
                    }
                    
                    ////////////////////////////////////////////////////////////////////
                    //Construct and emplace Roost
                    i.Emit(OpCodes.Ldloc, vMethodInfo);
                    i.Emit(OpCodes.Ldloc, vCuckoos);
                    i.Emit(OpCodes.Newobj, R.Roost_mCtor);
                    i.Emit(OpCodes.Stsfld, fRoostRef);


                    ////////////////////////////////////////////////////////////////////
                    //Init Cuckoos ////////////////////////////////////////////////////
                    for(int iC = 0; iC < _spec.CuckooAttributes.Length; iC++) {
                        i.Emit(OpCodes.Ldloc, vCuckoos);
                        i.Emit(OpCodes.Ldc_I4, iC);
                        i.Emit(OpCodes.Ldelem_Ref);

                        i.Emit(OpCodes.Ldloc, vMethodInfo);
                        i.Emit(OpCodes.Callvirt, R.ICuckoo_mInit);
                    }
                });

            return fRoostRef;
        }






        MethodDefinition UsurpOuterMethod(WeaveContext ctx, CallWeaver topCallWeaver) {
            var mOuter = ctx.mOuter;
            var R = ctx.RefMap;
            var fRoost = ctx.fRoost;
            var tCont = ctx.tCont;
            var tContRef = ctx.tContRef;

            var fRoostRef = tCont.HasGenericParameters
                                ? fRoost.CloneWithNewDeclaringType(tContRef)
                                : fRoost;

            var mOuterRef = mOuter.HasGenericParameters
                            ? mOuter.MakeGenericInstanceMethod(mOuter.GenericParameters.ToArray())
                            : (MethodReference)mOuter;

            var call = topCallWeaver.Weave(mOuterRef);


            var contGenArgs = tContRef is GenericInstanceType
                                ? ((GenericInstanceType)tContRef).GenericArguments.ToArray()
                                : new TypeReference[0];

            var methodGenArgs = mOuterRef is GenericInstanceMethod
                                ? ((GenericInstanceMethod)mOuterRef).GenericArguments.ToArray()
                                : new TypeReference[0];

            var Call_TypeRef = call.tCall.HasGenericParameters
                                ? call.tCall.MakeGenericInstanceType(contGenArgs.Concat(methodGenArgs).ToArray())
                                : (TypeReference)call.tCall;

            var Call_mCtor = Call_TypeRef.ReferenceMethod(m => m.IsConstructor);
            var Call_fReturnRef = Call_TypeRef.ReferenceField(call.fReturn.Name);


            //if(tTopCall.HasGenericParameters) {             //maybe we need to be more explicit about various refs n gen instances
            //    var rtCallGenTypes = tCont.GenericParameters
            //                                    .Concat(mOuter.GenericParameters)
            //                                    .ToArray();
                
            //    tTopCall = tTopCall.MakeGenericInstanceType(rtCallGenTypes);
            //}

            //var TopCall_mCtor = tTopCall.ReferenceMethod(c => c.IsConstructor);
            //var TopCall_fReturnRef = tTopCall.ReferenceField("_return");

            mOuter.Body = new MethodBody(mOuter);

            mOuter.Compose(
                (i, m) => {
                    var vCall = m.Body.AddVariable<ICall>();

                    i.Emit(OpCodes.Ldsfld, fRoostRef);

                    if(m.HasThis) {
                        i.Emit(OpCodes.Ldarg_0);
                    }
                    else {
                        i.Emit(OpCodes.Ldnull);
                    }

                    foreach(var param in m.Parameters) {
                        i.Emit(OpCodes.Ldarg_S, param);

                        if(param.ParameterType.IsByReference) {
                            i.Emit(OpCodes.Ldobj, param.ParameterType.GetElementType());
                        }
                    }

                    i.Emit(OpCodes.Newobj, Call_mCtor);
                    i.Emit(OpCodes.Stloc_S, vCall);

                    i.Emit(OpCodes.Ldsfld, fRoostRef);
                    i.Emit(OpCodes.Call, R.Roost_mGetUsurpers);
                    i.Emit(OpCodes.Ldc_I4_0);
                    i.Emit(OpCodes.Ldelem_Ref);

                    i.Emit(OpCodes.Ldloc, vCall);

                    i.Emit(OpCodes.Callvirt, R.ICuckoo_mUsurp);
                    
                    //access all the byref arg fields, and copy values back to addresses given by mOuter args
                    //...

                    if(Call_fReturnRef != null) {
                        i.Emit(OpCodes.Ldloc, vCall);
                        i.Emit(OpCodes.Castclass, Call_TypeRef);
                        i.Emit(OpCodes.Ldfld, Call_fReturnRef);
                    }

                    i.Emit(OpCodes.Ret);
                });

            return mOuter;
        }




    }
}
