using Cuckoo.Common;
using Cuckoo.Fody.Cecil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Linq;

namespace Cuckoo.Fody
{
    using Refl = System.Reflection;
    
    abstract class CallClassWeaver
    {
        protected WeaveContext _ctx;
        protected ScopeTypeMapper _types;

        protected FieldDefinition _fInstance;
        protected FieldDefinition _fCallSite;
        protected FieldDefinition _fReturn;
        protected FieldDefinition[] _rfArgValues;


        public CallClassWeaver(WeaveContext ctx) {
            _ctx = ctx;
        }


        protected abstract void EmitInnerInvoke(ILProcessor il);


        public TypeDefinition Build() {
            var R = _ctx.RefMap;
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

            _types = new ScopeTypeMapper(t);


            _fCallSite = t.AddField<Cuckoo.Common.CallSite>("_callSite");
            _fInstance = t.AddField<object>("_instance");
            
            if(mOuter.ReturnsValue()) {
                _fReturn = t.AddField(_types.Map(mOuter.ReturnType), "_return");
                _fReturn.Attributes = FieldAttributes.Public;
            }


            _rfArgValues = mOuter.Parameters
                                    .Select(p => t.AddField(
                                                        _types.Map(p.ParameterType), 
                                                        "_argValue_" + p.Name
                                                        ) )
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
                    i.Emit(OpCodes.Call, R.Object_mCtor);
                    
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
                            if(_fReturn != null) {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, _fReturn);

                                if(_fReturn.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Box, _fReturn.FieldType);
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
                            if(_fReturn != null) {
                                i.Emit(OpCodes.Ldarg_0);

                                i.Emit(OpCodes.Ldarg_1);

                                if(_fReturn.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Unbox_Any, _fReturn.FieldType);
                                }
                                else {
                                    i.Emit(OpCodes.Castclass, _fReturn.FieldType);
                                }

                                i.Emit(OpCodes.Stfld, _fReturn);
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
                                else {
                                    i.Emit(OpCodes.Castclass, fArgValue.FieldType);
                                }

                                i.Emit(OpCodes.Stfld, fArgValue);

                                i.Append(lbSkipArgUpdate);

                                iA++;
                            }

                            i.Append(lbSkipAllArgUpdates);

                            EmitInnerInvoke(i);

                            if(_fReturn != null) {
                                var vReturn = m.Body.AddVariable(_fReturn.FieldType);
                                i.Emit(OpCodes.Stloc, vReturn);

                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldloc, vReturn);
                                i.Emit(OpCodes.Stfld, _fReturn);
                            }

                            i.Emit(OpCodes.Ret);
                        });

            return t;
        }
    }


    class MediateCallClassWeaver : CallClassWeaver
    {
        TypeDefinition _tNextCall;
        int _iUsurper;

        public MediateCallClassWeaver(WeaveContext ctx, TypeDefinition tNextCall, int iUsurper)
            : base(ctx) 
        {
            _tNextCall = tNextCall;
            _iUsurper = iUsurper;
        }

        protected override void EmitInnerInvoke(ILProcessor i) {
            var R = _ctx.RefMap;
            var NextCall_mCtor = _tNextCall.GetConstructors().First();
            var NextCall_fReturn = _tNextCall.Fields.FirstOrDefault(f => f.Name == "_return");

            //get next usurper by index
            var vUsurper = i.Body.AddVariable<ICallUsurper>();
            var vCallSite = i.Body.AddVariable<Cuckoo.Common.CallSite>();
            var vCall = i.Body.AddVariable<ICall>();

            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fCallSite);
            i.Emit(OpCodes.Call, R.CallSite_mGetUsurpers);
            i.Emit(OpCodes.Ldc_I4, _iUsurper);
            i.Emit(OpCodes.Ldelem_Ref);
            i.Emit(OpCodes.Stloc_S, vUsurper);
            
            //load various args for next call ctor
            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fCallSite);

            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fInstance);

            foreach(var fArgValue in _rfArgValues) {
                i.Emit(OpCodes.Ldarg_0);                
                i.Emit(OpCodes.Ldfld, fArgValue);
            }

            i.Emit(OpCodes.Newobj, NextCall_mCtor);
            i.Emit(OpCodes.Stloc_S, vCall);

            //feed to next usurper
            i.Emit(OpCodes.Ldloc_S, vUsurper);
            i.Emit(OpCodes.Ldloc_S, vCall);
            i.Emit(OpCodes.Callvirt, R.ICallUsurper_mUsurp);

            if(NextCall_fReturn != null) {
                i.Emit(OpCodes.Ldloc, vCall); 
                i.Emit(OpCodes.Castclass, _tNextCall);
                i.Emit(OpCodes.Ldfld, NextCall_fReturn);
            }
        }
    }


    class FinalCallClassWeaver : CallClassWeaver
    {
        public FinalCallClassWeaver(WeaveContext ctx)
            : base(ctx) { }

        protected override void EmitInnerInvoke(ILProcessor i) {
            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fInstance);
            i.Emit(OpCodes.Castclass, _ctx.InnerMethod.DeclaringType);

            foreach(var fArgValue in _rfArgValues) {
                i.Emit(OpCodes.Ldarg_0);
                i.Emit(OpCodes.Ldfld, fArgValue);
            }

            i.Emit(OpCodes.Call, _ctx.InnerMethod);
        }
    }

}
