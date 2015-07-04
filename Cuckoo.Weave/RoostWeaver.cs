using Cuckoo.Weave.Cecil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cuckoo.Weave
{
    using Refl = System.Reflection;

    internal partial class RoostWeaver
    {
        RoostWeaveSpec _spec;
        Action<string> _logger;

        public RoostWeaver(RoostWeaveSpec spec, Action<string> logger) 
        {
            _spec = spec;
            _logger = logger;
        }


        WeaveContext CreateContext(RoostWeaveSpec spec) {
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
                RefMap = new CommonRefs(module, spec.Method),
                Logger = _logger
            };
        }

        

        public void Weave() 
        {   
            var ctx = CreateContext(_spec);

            var mInner = TransplantOuterToInner(ctx);
            
            var mOuter = WeaveOuterMethod(ctx, _spec.WeaveProvSpecs, mInner);
            
            AddCuckooedAttribute(ctx, mOuter, mInner);

            //ctx.Logger(string.Format("Mod applied to {0}!", ctx.mOuter.FullName));
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
                                        m.Name = names.GetElementName("Cuckooed", mOuter.Name);

                                        m.Attributes ^= MethodAttributes.Public | MethodAttributes.Private;

                                        m.Attributes &= ~MethodAttributes.Virtual
                                                        & ~MethodAttributes.NewSlot;

                                        if(mOuter.IsConstructor) {
                                            m.Attributes &= ~MethodAttributes.SpecialName 
                                                            & ~MethodAttributes.RTSpecialName;

                                            GetInitialCtorInsts(m.Body).ToList()
                                                .ForEach(i => m.Body.Instructions.Remove(i));
                                        }
                                    });

            ctx.mInner = mInner;

            return mInner;
        }


        Instruction[] GetInitialCtorInsts(MethodBody methodBody) {
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
                                }).ToArray();
        }




        MethodDefinition WeaveOuterMethod(
                                WeaveContext ctx, 
                                IEnumerable<ProvWeaveSpec> cuckoos, 
                                MethodDefinition mInner) 
        {            
            var R = ctx.RefMap;
            var mOuter = ctx.mOuter;
            var tCont = ctx.tCont;
            var tContRef = ctx.tContRef;

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
            
            var call = callWeaver.Weave(mOuterRef, args, _spec);
            
            if(call.RequiresInstanciation) {
                call = call.Instanciate(contGenArgs, methodGenArgs);     
            }


            var initialCtorInsts = mOuter.IsConstructor
                                            ? GetInitialCtorInsts(mOuter.Body)
                                            : null;
                        

            mOuter.Body = new MethodBody(mOuter);

            mOuter.Compose(
                (i, m) => {
                    var vCall = m.Body.AddVariable(call.Type);
                    var vParams = m.Body.AddVariable<Refl.ParameterInfo[]>();
                    var vCallArgs = m.Body.AddVariable<ICallArg[]>();
                    var vArgFlags = m.Body.AddVariable<ulong>();
                    
                    i.Emit(OpCodes.Ldsfld, call.RoostField);

                    i.Emit(OpCodes.Dup);
                    i.Emit(OpCodes.Call, R.Roost_mGetParams);
                    i.Emit(OpCodes.Stloc_S, vParams);

                    i.Emit(OpCodes.Newobj, call.CtorMethod);
                    i.Emit(OpCodes.Stloc_S, vCall);
                    

                    i.Emit(OpCodes.Ldloc_S, vCall);

                    i.Emit(OpCodes.Ldc_I4, args.Length);
                    i.Emit(OpCodes.Newarr, R.ICallArg_Type);

                    foreach(var arg in args) {
                        i.Emit(OpCodes.Dup);
                        i.Emit(OpCodes.Ldc_I4, arg.Index);

                        i.Emit(OpCodes.Ldloc_S, vParams);
                        i.Emit(OpCodes.Ldc_I4, arg.Index);
                        i.Emit(OpCodes.Ldelem_Ref);

                        i.Emit(OpCodes.Ldc_I4, arg.Index);

                        i.Emit(OpCodes.Ldloc_S, vCall);

                        if(arg.IsByRef) {
                            i.Emit(OpCodes.Ldarg_S, arg.OuterParam.Resolve());
                            i.Emit(OpCodes.Ldobj, arg.Type);
                        }
                        else {
                            i.Emit(OpCodes.Ldarg_S, arg.OuterParam.Resolve());
                        }

                        i.Emit(OpCodes.Newobj, arg.CallArg_mCtor);

                        i.Emit(OpCodes.Stelem_Ref);
                    }

                    i.Emit(OpCodes.Dup);
                    i.Emit(OpCodes.Stloc_S, vCallArgs);

                    i.Emit(OpCodes.Callvirt, call.PrepareMethod);
                                        
                    if(mOuter.IsConstructor) {
                        i.Emit(OpCodes.Ldloc_S, vCall);
                        i.Emit(OpCodes.Ldfld, call.ArgFlagsField);
                        i.Emit(OpCodes.Stloc_S, vArgFlags);

                        foreach(var inst in initialCtorInsts) {
                            i.Append(inst);
                        }

                        foreach(var callArg in call.Args) {
                            int argIndexAdj = callArg.MethodArg.Index + 1;

                            var insts = initialCtorInsts
                                            .Where(n => (n.OpCode.Name == "ldarg" && n.Operand.Equals(argIndexAdj)) //better testing here...
                                                        || (n.OpCode.Name == "ldarg." + argIndexAdj.ToString()))
                                            .ToArray();

                            foreach(var inst in insts) {
                                var cursor = i.Create(OpCodes.Nop);
                                i.Replace(inst, cursor);
                            
                                var lbLoadArg = i.Create(OpCodes.Nop);
                                var lbEnd = i.Create(OpCodes.Nop);

                                i.InsertBefore(cursor, i.Create(OpCodes.Ldloc_S, vArgFlags));
                                i.InsertBefore(cursor, i.Create(OpCodes.Ldc_I8, (long)(1L << callArg.MethodArg.Index)));
                                i.InsertBefore(cursor, i.Create(OpCodes.Or));
                                i.InsertBefore(cursor, i.Create(OpCodes.Brfalse_S, lbLoadArg));
                                
                                i.InsertBefore(cursor, i.Create(OpCodes.Ldloc_S, vCallArgs));
                                i.InsertBefore(cursor, i.Create(OpCodes.Ldc_I4, callArg.MethodArg.Index));
                                i.InsertBefore(cursor, i.Create(OpCodes.Ldelem_Ref));
                                i.InsertBefore(cursor, i.Create(OpCodes.Castclass, callArg.CallArg_Type));
                                i.InsertBefore(cursor, i.Create(OpCodes.Ldfld, callArg.CallArg_fValue));
                                i.InsertBefore(cursor, i.Create(OpCodes.Br_S, lbEnd));

                                i.InsertBefore(cursor, lbLoadArg);
                                i.InsertBefore(cursor, i.Create(OpCodes.Ldarg, argIndexAdj));

                                i.InsertBefore(cursor, lbEnd);

                                i.Remove(cursor);
                            }
                        }
                    }


                    i.Emit(OpCodes.Ldloc_S, vCall);

                    if(m.IsStatic) {
                        i.Emit(OpCodes.Ldnull);
                    }
                    else {    
                        i.Emit(OpCodes.Ldarg_0);

                        if(tCont.IsValueType) {
                            i.Emit(OpCodes.Ldobj, tContRef);
                        }
                    }

                    i.Emit(OpCodes.Callvirt, call.InvokeMethod);


                    foreach(var callArg in call.Args.Where(a => a.IsByRef)) {
                        i.Emit(OpCodes.Ldarg_S, callArg.MethodArg.OuterParam.Resolve());
                        i.Emit(OpCodes.Ldloc_S, vCall);
                        i.Emit(OpCodes.Ldfld, call.ArgsField);
                        i.Emit(OpCodes.Ldc_I4, callArg.Index);
                        i.Emit(OpCodes.Ldelem_Ref);
                        i.Emit(OpCodes.Castclass, callArg.CallArg_Type);
                        i.Emit(OpCodes.Ldfld, callArg.CallArg_fValue);
                        i.Emit(OpCodes.Stobj, callArg.Type);
                    }

                    if(tCont.IsValueType) {
                        i.Emit(OpCodes.Ldarg_0);
                        i.Emit(OpCodes.Ldloc_S, vCall);
                        i.Emit(OpCodes.Ldfld, call.InstanceField);
                        i.Emit(OpCodes.Stobj, tContRef);
                    }

                    if(call.ReturnsValue) {
                        i.Emit(OpCodes.Ldloc, vCall);
                        i.Emit(OpCodes.Ldfld, call.ReturnField);
                    }

                    i.Emit(OpCodes.Ret);
                });

            return mOuter;
        }

    }
}
