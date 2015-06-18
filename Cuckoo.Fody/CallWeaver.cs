using Cuckoo;
using Cuckoo.Fody.Cecil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Linq;

namespace Cuckoo.Fody
{
    using Cuckoo.Impl;
    using Refl = System.Reflection;
    
    abstract class CallWeaver
    {
        protected WeaveContext _ctx;
        protected ScopeTypeMapper _types;

        protected TypeDefinition _tCall;
        protected TypeReference _tCallRef;

        protected TypeDefinition _tCont;
        protected TypeReference _tContRef;

        protected MethodDefinition _mCtor;
        protected MethodReference _mCtorRef;

        protected FieldDefinition _fInstance;
        protected FieldReference _fInstanceRef;

        protected FieldDefinition _fRoost;
        protected FieldReference _fRoostRef;

        protected FieldDefinition _fReturn;
        protected FieldReference _fReturnRef;

        protected FieldReference _fArgsChanged;


        protected class Arg
        {
            public bool IsByRef { get; set; }
            public int Index { get; set; }
            public ParameterDefinition MethodParam { get; set; }
            public ParameterDefinition CtorParam { get; set; }
            public FieldDefinition Field { get; set; }
            public FieldReference FieldRef { get; set; }
            public TypeReference ParamType { get { return MethodParam.ParameterType; } }
            public TypeReference FieldType { get { return Field.FieldType; } }

            public TypeReference CallArg_Type { get; set; }
            public MethodReference CallArg_mCtor { get; set; }
            public MethodReference CallArg_mHasChanged { get; set; }
            public MethodReference CallArg_mGetTypedValue { get; set; }
            public MethodReference CallArg_mSetTypedValue { get; set; }
        }

        protected Arg[] _args;
        protected Arg[] _byrefArgs;



        public CallWeaver(WeaveContext ctx) {
            _ctx = ctx;
        }


        protected abstract void EmitInnerInvoke(ILProcessor il, MethodReference mOuterRef);



        protected TypeReference[] _rtContGenArgs;
        protected TypeReference[] _rtMethodGenArgs;


        public CallInfo Weave(MethodReference mOuterRef) {
            var R = _ctx.RefMap;
            var mod = _ctx.Module;
            _tCont = _ctx.tCont;
            _tContRef = _ctx.tContRef;
            



            string callClassName = _ctx.NameSource.GetElementName("CALL", mOuterRef.Name);

            _tCall = new TypeDefinition(
                                _tCont.Namespace,
                                callClassName,
                                TypeAttributes.Class | TypeAttributes.NestedPrivate | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass
                                );

            _tCont.NestedTypes.Add(_tCall);

            _tCall.BaseType = mod.TypeSystem.Object;
            _tCall.Interfaces.Add(R.ICall_Type);



            _rtContGenArgs = _tContRef is GenericInstanceType
                                ? ((GenericInstanceType)_tContRef).GenericArguments.ToArray()
                                : new TypeReference[0];

            _rtMethodGenArgs = mOuterRef is GenericInstanceMethod
                                ? ((GenericInstanceMethod)mOuterRef).GenericArguments.ToArray()
                                : new TypeReference[0];


            _types = new ScopeTypeMapper(_tCall);

            foreach(var tContGenArg in _rtContGenArgs) {
                _types.Map(tContGenArg);
            }

            foreach(var tMethodGenArg in _rtMethodGenArgs) {
                _types.Map(tMethodGenArg);
            }



            _fRoost = _tCall.AddField<Roost>("_roost");
            _fInstance = _tCall.AddField<object>("_instance");
            
            if(mOuterRef.ReturnsValue()) {
                _fReturn = _tCall.AddField(_types.Map(mOuterRef.ReturnType), "_return");
                _fReturn.Attributes = FieldAttributes.Public;
            }

            int iArg = 0;

            _args = mOuterRef.Parameters
                                .Select(p => {                                    
                                    var argType = _types.Map(p.ParameterType).GetElementType();

                                    var tCallArg = R.CallArg_Type.MakeGenericInstanceType(argType);
                                    
                                    return new Arg() {
                                                Index = iArg++,
                                                MethodParam = p,
                                                CtorParam = new ParameterDefinition(argType),
                                                IsByRef = p.ParameterType.IsByReference,
                                                Field = _tCall.AddField(argType,
                                                                            "_arg_" + p.Name,
                                                                            FieldAttributes.Public ),
                                                CallArg_Type = tCallArg,
                                                CallArg_mCtor = tCallArg.ReferenceMethod(m => m.IsConstructor),
                                                CallArg_mHasChanged = tCallArg.ReferenceMethod(R.CallArg_mGetHasChanged.Name),
                                                CallArg_mGetTypedValue = tCallArg.ReferenceMethod(R.CallArg_mGetTypedValue.Name),
                                                CallArg_mSetTypedValue = tCallArg.ReferenceMethod(R.CallArg_mSetTypedValue.Name)
                                            };
                                }) 
                                .ToArray();

            _byrefArgs = _args.Where(a => a.IsByRef)
                                .ToArray();


            _tCallRef = _tCall.HasGenericParameters
                        ? _tCall.MakeGenericInstanceType(_tCall.GenericParameters.ToArray())
                        : (TypeReference)_tCall;
            
            _fInstanceRef = _tCallRef.ReferenceField(_fInstance.Name);

            _fRoostRef = _tCallRef.ReferenceField(_fRoost.Name);

            if(_fReturn != null) {
                _fReturnRef = _tCallRef.ReferenceField(_fReturn.Name);
            }

            foreach(var arg in _args) {
                arg.FieldRef = _tCallRef.ReferenceField(arg.Field.Name);
            }



           

            _tCall.OverrideMethod(
                        "get_Instance",
                        R.ICall_Type,
                        (i, m) => {
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, _fInstanceRef);
                            i.Emit(OpCodes.Ret);
                        });

            _tCall.OverrideMethod(
                        "get_Method",
                        R.ICall_Type,
                        (i, m) => {
                            var Roost_mGetMethod = mod.ImportReference(
                                                                R.Roost_Type.Resolve().GetMethod("get_Method"));
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, _fRoostRef);
                            i.Emit(OpCodes.Call, Roost_mGetMethod);
                            i.Emit(OpCodes.Ret);
                        });


            var fCallArgs = _tCall.AddField<ICallArg[]>("_rCallArgs");
            var fCallArgsRef = _tCallRef.ReferenceField(fCallArgs.Name);

            _tCall.OverrideMethod(
                        "get_Args",
                        R.ICall_Type,
                        (i, m) => {
                            var vArgs = m.Body.AddVariable<ICallArg[]>();
                            var vParams = m.Body.AddVariable<Refl.ParameterInfo[]>();
                            var lbCreateArgs = i.Create(OpCodes.Nop);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCallArgsRef);
                            i.Emit(OpCodes.Stloc_S, vArgs);
                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ldnull);
                            i.Emit(OpCodes.Ceq);
                            i.Emit(OpCodes.Brtrue_S, lbCreateArgs);

                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ret);

                            i.Append(lbCreateArgs);

                            i.Emit(OpCodes.Ldc_I4, _args.Length);
                            i.Emit(OpCodes.Newarr, R.ICallArg_Type);
                            i.Emit(OpCodes.Stloc_S, vArgs);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, _fRoostRef);
                            i.Emit(OpCodes.Call, R.Roost_mGetParams);
                            i.Emit(OpCodes.Stloc_S, vParams);

                            foreach(var arg in _args) {
                                i.Emit(OpCodes.Ldloc_S, vArgs);
                                i.Emit(OpCodes.Ldc_I4, arg.Index);

                                i.Emit(OpCodes.Ldloc_S, vParams);
                                i.Emit(OpCodes.Ldc_I4, arg.Index);
                                i.Emit(OpCodes.Ldelem_Ref);

                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, arg.FieldRef);

                                i.Emit(OpCodes.Newobj, arg.CallArg_mCtor);

                                i.Emit(OpCodes.Stelem_Ref);
                            }

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Stfld, fCallArgsRef);

                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ret);
                        });

            _tCall.OverrideMethod(
                        "get_ReturnValue",
                        R.ICall_Type,
                        (i, m) => {
                            if(_fReturn != null) {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, _fReturnRef);
                                i.Emit(OpCodes.Box, _fReturnRef.FieldType);
                            }
                            else {
                                i.Emit(OpCodes.Ldnull);
                            }

                            i.Emit(OpCodes.Ret);
                        });

            _tCall.OverrideMethod(
                        "set_ReturnValue",
                        R.ICall_Type,
                        (i, m) => {
                            if(_fReturn != null) {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldarg_1);
                                i.Emit(OpCodes.Unbox_Any, _fReturnRef.FieldType);
                                i.Emit(OpCodes.Stfld, _fReturnRef);
                            }

                            i.Emit(OpCodes.Ret);
                        });








            _mCtor = _tCall.AddCtor(
                        new[] {
                            new ParameterDefinition(R.Roost_Type),
                            new ParameterDefinition(mod.TypeSystem.Object)
                            }
                            .Concat(_args.Select(a => a.CtorParam)),
                        (i, m) => {
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Call, R.Object_mCtor);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldarg_1);
                            i.Emit(OpCodes.Stfld, _fRoostRef);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldarg_2);
                            i.Emit(OpCodes.Stfld, _fInstanceRef);

                            foreach(var arg in _args) {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldarg_S, arg.CtorParam);
                                i.Emit(OpCodes.Stfld, arg.FieldRef);
                            }

                            i.Emit(OpCodes.Ret);
                        });
            


            _tCall.OverrideMethod(
                        "CallInner",
                        R.ICall_Type,
                        (i, m) => {
                            var vCallArgs = i.Body.AddVariable<ICallArg[]>();
                            var vCallArg = i.Body.AddVariable<ICallArg>();

                            var lbSkipAllArgs = i.Create(OpCodes.Nop);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCallArgsRef);
                            i.Emit(OpCodes.Stloc_S, vCallArgs);

                            i.Emit(OpCodes.Ldloc_S, vCallArgs);
                            i.Emit(OpCodes.Ldnull);
                            i.Emit(OpCodes.Ceq);
                            i.Emit(OpCodes.Brtrue, lbSkipAllArgs);

                            foreach(var arg in _args) {
                                var lbSkipThisArg = i.Create(OpCodes.Nop);

                                i.Emit(OpCodes.Ldloc_S, vCallArgs);
                                i.Emit(OpCodes.Ldc_I4, arg.Index);
                                i.Emit(OpCodes.Ldelem_Ref);
                                i.Emit(OpCodes.Stloc_S, vCallArg);

                                i.Emit(OpCodes.Ldloc_S, vCallArg);
                                i.Emit(OpCodes.Castclass, arg.CallArg_Type);
                                i.Emit(OpCodes.Call, arg.CallArg_mHasChanged);
                                i.Emit(OpCodes.Brfalse_S, lbSkipThisArg);

                                    i.Emit(OpCodes.Ldarg_0);
                                    i.Emit(OpCodes.Ldloc_S, vCallArg);
                                    i.Emit(OpCodes.Castclass, arg.CallArg_Type);
                                    i.Emit(OpCodes.Call, arg.CallArg_mGetTypedValue);
                                    i.Emit(OpCodes.Stfld, arg.FieldRef);
                                
                                i.Append(lbSkipThisArg);
                            }

                            i.Append(lbSkipAllArgs);


                            EmitInnerInvoke(i, mOuterRef);


                            var lbSkipAllArgs2 = i.Create(OpCodes.Nop);

                            i.Emit(OpCodes.Ldloc_S, vCallArgs);
                            i.Emit(OpCodes.Ldnull);
                            i.Emit(OpCodes.Ceq);
                            i.Emit(OpCodes.Brtrue_S, lbSkipAllArgs2);

                            foreach(var arg in _byrefArgs) {
                                i.Emit(OpCodes.Ldloc_S, vCallArgs);
                                i.Emit(OpCodes.Ldc_I4, arg.Index);
                                i.Emit(OpCodes.Ldelem_Ref);
                                i.Emit(OpCodes.Castclass, arg.CallArg_Type);

                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, arg.FieldRef);

                                i.Emit(OpCodes.Call, arg.CallArg_mSetTypedValue);
                            }

                            i.Append(lbSkipAllArgs2);


                            if(_fReturn != null) {
                                var vReturn = m.Body.AddVariable(_fReturnRef.FieldType);

                                i.Emit(OpCodes.Stloc, vReturn);

                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldloc, vReturn);
                                i.Emit(OpCodes.Stfld, _fReturnRef);
                            }

                            i.Emit(OpCodes.Ret);
                        });



            var mAfterUsurp = _tCall.AddMethod(
                                        "AfterUsurp",
                                        new ParameterDefinition[0],
                                        mod.TypeSystem.Void,
                                        (i, m) => {
                                            var vCallArgs = i.Body.AddVariable<ICallArg[]>();
                                            var vCallArg = i.Body.AddVariable<ICallArg>();

                                            var lbSkipAll = i.Create(OpCodes.Nop);

                                            i.Emit(OpCodes.Ldarg_0);
                                            i.Emit(OpCodes.Ldfld, fCallArgsRef);
                                            i.Emit(OpCodes.Stloc_S, vCallArgs);

                                            i.Emit(OpCodes.Ldloc_S, vCallArgs);
                                            i.Emit(OpCodes.Ldnull);
                                            i.Emit(OpCodes.Ceq);
                                            i.Emit(OpCodes.Brtrue, lbSkipAll);

                                            foreach(var arg in _byrefArgs) {
                                                var lbSkipThis = i.Create(OpCodes.Nop);

                                                i.Emit(OpCodes.Ldloc_S, vCallArgs);
                                                i.Emit(OpCodes.Ldc_I4, arg.Index);
                                                i.Emit(OpCodes.Ldelem_Ref);
                                                i.Emit(OpCodes.Stloc_S, vCallArg);

                                                i.Emit(OpCodes.Ldloc_S, vCallArg);
                                                i.Emit(OpCodes.Castclass, arg.CallArg_Type);
                                                i.Emit(OpCodes.Call, arg.CallArg_mHasChanged);
                                                i.Emit(OpCodes.Brfalse_S, lbSkipThis);

                                                i.Emit(OpCodes.Ldarg_0);
                                                i.Emit(OpCodes.Ldloc_S, vCallArg);
                                                i.Emit(OpCodes.Castclass, arg.CallArg_Type);
                                                i.Emit(OpCodes.Call, arg.CallArg_mGetTypedValue);
                                                i.Emit(OpCodes.Stfld, arg.FieldRef);

                                                i.Append(lbSkipThis);
                                            }

                                            i.Append(lbSkipAll);
                                            i.Emit(OpCodes.Ret);
                                        });


            return new CallInfo(_tCall,
                                    _mCtor,
                                    mAfterUsurp,
                                    _fReturn,
                                    _args.Select(a => new CallArgInfo(
                                                            a.Field,
                                                            a.MethodParam,
                                                            a.CtorParam
                                                            ))
                                    );
        }
    }



    class MediateCallWeaver : CallWeaver
    {
        CallWeaver _nextCallWeaver;
        int _iCuckoo;

        public MediateCallWeaver(WeaveContext ctx, CallWeaver nextCallWeaver, int iCuckoo)
            : base(ctx) 
        {
            _nextCallWeaver = nextCallWeaver;
            _iCuckoo = iCuckoo;
        }

        protected override void EmitInnerInvoke(ILProcessor i, MethodReference mOuterRef) {
            var R = _ctx.RefMap;

            var nextCall = _nextCallWeaver.Weave(mOuterRef);

            if(nextCall.RequiresInstanciation) {
                nextCall = nextCall.Instanciate(((GenericInstanceType)_tCallRef).GenericArguments);
            }
            
            //get next usurper by index
            var vCuckoo = i.Body.AddVariable<ICuckoo>();
            var vRoost = i.Body.AddVariable<Roost>();
            var vCall = i.Body.AddVariable(nextCall.Type);

            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fRoostRef);
            i.Emit(OpCodes.Call, R.Roost_mGetUsurpers);
            i.Emit(OpCodes.Ldc_I4, _iCuckoo);
            i.Emit(OpCodes.Ldelem_Ref);
            i.Emit(OpCodes.Stloc_S, vCuckoo);
            
            //load args and construct NextCall
            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fRoostRef);

            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fInstanceRef);

            foreach(var arg in _args) {
                i.Emit(OpCodes.Ldarg_0);
                i.Emit(OpCodes.Ldfld, arg.FieldRef);
            }

            i.Emit(OpCodes.Newobj, nextCall.CtorMethod);
            i.Emit(OpCodes.Stloc_S, vCall);

            i.Emit(OpCodes.Ldloc_S, vCuckoo);
            i.Emit(OpCodes.Ldloc_S, vCall);
            i.Emit(OpCodes.Callvirt, R.ICuckoo_mOnCall);

            i.Emit(OpCodes.Ldloc, vCall);
            i.Emit(OpCodes.Call, nextCall.AfterUsurpMethod);

            foreach(var nextCallArg in nextCall.Args.Where(a => a.IsByRef)) {
                i.Emit(OpCodes.Ldarg_0);
                i.Emit(OpCodes.Ldloc_S, vCall);
                i.Emit(OpCodes.Ldfld, nextCallArg.Field);
                i.Emit(OpCodes.Stfld, _args.First(a => a.Field.Name == nextCallArg.Field.Name).FieldRef); //!!!!!!!!!!!!!!!!!!
            }

            if(nextCall.ReturnsValue) {
                i.Emit(OpCodes.Ldloc, vCall); 
                i.Emit(OpCodes.Ldfld, nextCall.ReturnField);
            }
        }
    }


    class FinalCallWeaver : CallWeaver
    {
        MethodDefinition _mInner;

        public FinalCallWeaver(WeaveContext ctx, MethodDefinition mInner)
            : base(ctx) 
        {
            _mInner = mInner;
        }

        protected override void EmitInnerInvoke(ILProcessor i, MethodReference mOuterRef) 
        {
            var mInnerRef = _mInner.CloneWithNewDeclaringType(_tContRef);

            if(mInnerRef.HasGenericParameters) {
                mInnerRef = mInnerRef.MakeGenericInstanceMethod(
                                        ((GenericInstanceType)_tCallRef).GenericArguments
                                                                            .Skip(_rtContGenArgs.Length)
                                                                            .ToArray());
            }
             
            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fInstanceRef);
            i.Emit(OpCodes.Castclass, _tContRef);

            foreach(var arg in _args) {
                i.Emit(OpCodes.Ldarg_0);

                if(arg.IsByRef) {
                    i.Emit(OpCodes.Ldflda, arg.FieldRef);
                }
                else {
                    i.Emit(OpCodes.Ldfld, arg.FieldRef);
                }
            }
            
            i.Emit(OpCodes.Call, mInnerRef);
        }
    }

}
