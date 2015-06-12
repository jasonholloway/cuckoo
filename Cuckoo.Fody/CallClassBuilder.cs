using Cuckoo.Common;
using Cuckoo.Fody.Cecil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace Cuckoo.Fody
{
    using Refl = System.Reflection;
    
    abstract class CallClassBuilder
    {
        protected BuildContext _ctx;

        protected FieldDefinition _fInstance;
        protected FieldDefinition _fCallSite;
        protected FieldDefinition[] _rfArgValues;


        public CallClassBuilder(BuildContext ctx) {
            _ctx = ctx;
        }


        protected abstract void EmitInvoke(ILProcessor il);


        public TypeDefinition Build() {
            var R = _ctx.Ref;
            var mod = _ctx.Module;
            var tContainer = _ctx.DeclaringType;
            var mOuter = _ctx.OuterMethod;

            string callClassName = _ctx.NameSource.GetElementName("CALL", mOuter.Name);

            var t = new TypeDefinition(
                                tContainer.Namespace,
                                callClassName,
                                TypeAttributes.Class | TypeAttributes.NestedPrivate
                                );

            tContainer.NestedTypes.Add(t);

            t.BaseType = mod.TypeSystem.Object;
            t.Interfaces.Add(R.ICall_TypeRef);

            _fCallSite = t.AddField<Cuckoo.Common.CallSite>("_callSite");
            _fInstance = t.AddField<object>("_instance");


            FieldDefinition fReturn = null;

            if(mOuter.ReturnsValue()) {
                fReturn = t.AddField(mOuter.ReturnType, "_return");
                fReturn.Attributes = FieldAttributes.Public;
            }


            _rfArgValues = mOuter.Parameters
                                    .Select(p => t.AddField(p.ParameterType, "_argValue_" + p.Name))
                                    .ToArray();

            var rCtorArgTypes = new[] { 
                                        R.CallSite_TypeRef,
                                        mod.TypeSystem.Object //instance
                                    }
                                .Concat(_rfArgValues.Select(f => f.FieldType))
                                .ToArray();

            t.AddCtor(
                rCtorArgTypes,
                (i, m) => {
                    i.Emit(OpCodes.Ldarg_0);
                    i.Emit(OpCodes.Ldarg_1);
                    i.Emit(OpCodes.Stfld, _fCallSite);

                    i.Emit(OpCodes.Ldarg_0);
                    i.Emit(OpCodes.Ldarg_2);
                    i.Emit(OpCodes.Stfld, _fInstance);

                    //put args into special fields
                    int iP = 2;
                    var rParams = m.Parameters.ToArray();

                    foreach(var fArgValue in _rfArgValues) {
                        i.Emit(OpCodes.Ldarg_0);
                        i.Emit(OpCodes.Ldarg_S, rParams[iP]);
                        i.Emit(OpCodes.Stfld, fArgValue);
                        iP++;
                    }

                    i.Emit(OpCodes.Ret);
                });

            t.OverrideMethod(
                        R.ICall_TypeRef,
                        "get_Instance",
                        (i, m) => {
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, _fInstance);
                            i.Emit(OpCodes.Ret);
                        });

            t.OverrideMethod(
                        R.ICall_TypeRef,
                        "get_Method",
                        (i, m) => {
                            var mCallSite_GetMethod = mod.ImportReference(
                                                                R.CallSite_TypeRef.Resolve().GetMethod("get_Method"));
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, _fCallSite);
                            i.Emit(OpCodes.Call, mCallSite_GetMethod);
                            i.Emit(OpCodes.Ret);
                        });


            var fCall_Args = t.AddField<CallArg[]>("_rArgs");

            t.OverrideMethod(
                        R.ICall_TypeRef,
                        "get_Args",
                        (i, m) => {
                            var vArgs = m.Body.AddVariable<CallArg[]>();
                            var vParams = m.Body.AddVariable<Refl.ParameterInfo[]>();
                            var lbCreateArgs = i.Create(OpCodes.Nop);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCall_Args);
                            i.Emit(OpCodes.Stloc_S, vArgs);
                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ldnull);
                            i.Emit(OpCodes.Ceq);
                            i.Emit(OpCodes.Brtrue_S, lbCreateArgs);

                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ret);

                            i.Append(lbCreateArgs);
                            i.Emit(OpCodes.Ldc_I4, _rfArgValues.Length);
                            i.Emit(OpCodes.Newarr, R.CallArg_TypeRef);
                            i.Emit(OpCodes.Stloc_S, vArgs);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, _fCallSite);
                            i.Emit(OpCodes.Call, R.CallSite_mGetParams);
                            i.Emit(OpCodes.Stloc_S, vParams);

                            for(int iA = 0; iA < _rfArgValues.Length; iA++) {
                                i.Emit(OpCodes.Ldloc_S, vArgs);
                                i.Emit(OpCodes.Ldc_I4, iA);

                                //load parameter
                                i.Emit(OpCodes.Ldloc_S, vParams);
                                i.Emit(OpCodes.Ldc_I4, iA);
                                i.Emit(OpCodes.Ldelem_Ref);

                                //load value & box
                                var fArgValue = _rfArgValues[iA];
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, fArgValue);
                                if(fArgValue.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Box, fArgValue.FieldType);
                                }

                                i.Emit(OpCodes.Newobj, R.CallArg_mCtor);

                                i.Emit(OpCodes.Stelem_Ref);
                            }

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Stfld, fCall_Args);

                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ret);
                        });

            t.OverrideMethod(
                        R.ICall_TypeRef,
                        "get_ReturnValue",
                        (i, m) => {
                            if(fReturn != null) {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, fReturn);

                                if(fReturn.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Box, fReturn.FieldType);
                                }
                            }
                            else {
                                i.Emit(OpCodes.Ldnull);
                            }

                            i.Emit(OpCodes.Ret);
                        });

            t.OverrideMethod(
                        R.ICall_TypeRef,
                        "set_ReturnValue",
                        (i, m) => {
                            if(fReturn != null) {
                                i.Emit(OpCodes.Ldarg_0);

                                i.Emit(OpCodes.Ldarg_1);

                                if(fReturn.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Unbox_Any, fReturn.FieldType);
                                }

                                i.Emit(OpCodes.Stfld, fReturn);
                            }

                            i.Emit(OpCodes.Ret);
                        });

            t.OverrideMethod(
                        R.ICall_TypeRef,
                        "CallInner",
                        (i, m) => {
                            ///////////////////////////////////////////////////////////////
                            //Update arg fields if args not pristine
                            var vArgs = i.Body.AddVariable<CallArg[]>();
                            var vArg = i.Body.AddVariable<CallArg>();
                            var lbSkipAllArgUpdates = i.Create(OpCodes.Nop);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCall_Args);
                            i.Emit(OpCodes.Stloc_S, vArgs);

                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ldnull);
                            i.Emit(OpCodes.Ceq);
                            i.Emit(OpCodes.Brtrue, lbSkipAllArgUpdates);

                            int iA = 0;

                            foreach(var fArgValue in _rfArgValues) {
                                var lbSkipArgUpdate = i.Create(OpCodes.Nop);

                                i.Emit(OpCodes.Ldloc_S, vArgs);
                                i.Emit(OpCodes.Ldc_I4, iA);
                                i.Emit(OpCodes.Ldelem_Ref);
                                i.Emit(OpCodes.Stloc_S, vArg);

                                i.Emit(OpCodes.Ldloc_S, vArg);
                                i.Emit(OpCodes.Call, R.CallArg_mGetIsPristine);
                                i.Emit(OpCodes.Brtrue_S, lbSkipArgUpdate);

                                //update arg value here
                                i.Emit(OpCodes.Ldarg_0);

                                i.Emit(OpCodes.Ldloc_S, vArg);
                                i.Emit(OpCodes.Call, R.CallArg_mGetValue);
                                if(fArgValue.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Unbox_Any, fArgValue.FieldType);
                                }

                                i.Emit(OpCodes.Stfld, fArgValue);

                                i.Append(lbSkipArgUpdate);

                                iA++;
                            }

                            i.Append(lbSkipAllArgUpdates);

                            EmitInvoke(i);

                            if(fReturn != null) {
                                var vReturn = m.Body.AddVariable(fReturn.FieldType);
                                i.Emit(OpCodes.Stloc, vReturn);

                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldloc, vReturn);
                                i.Emit(OpCodes.Stfld, fReturn);
                            }

                            i.Emit(OpCodes.Ret);
                        });

            return t;
        }
    }


    class MediateCallClassBuilder : CallClassBuilder
    {
        public MediateCallClassBuilder(BuildContext ctx)
            : base(ctx) { }

        protected override void EmitInvoke(ILProcessor il) {
            //need to create next call class
            //and feed it to next usurper
            //...
        }
    }


    class FinalCallClassBuilder : CallClassBuilder
    {
        public FinalCallClassBuilder(BuildContext ctx)
            : base(ctx) { }

        protected override void EmitInvoke(ILProcessor i) {
            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fInstance);

            foreach(var fArgValue in _rfArgValues) {
                i.Emit(OpCodes.Ldarg_0);
                i.Emit(OpCodes.Ldfld, fArgValue);
            }

            i.Emit(OpCodes.Call, _ctx.InnerMethod);
        }
    }

}
