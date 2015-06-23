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

    internal partial class CallWeaver
    {
        WeaveContext _ctx;

        public CallWeaver(WeaveContext ctx) {
            _ctx = ctx;
        }

        public CallInfo Weave(MethodReference mOuterRef, MethodWeaver.ArgSpec[] methodArgs) 
        {
            var R = _ctx.RefMap;
            var mod = _ctx.Module;
            var tCont = _ctx.tCont;
            var tContRef = _ctx.tContRef;
            string methodName = _ctx.mOuter.Name;

            bool isStaticMethod = mOuterRef.Resolve().IsStatic;
            bool returnsValue = mOuterRef.ReturnsValue();


            
            string callClassName = _ctx.NameSource.GetElementName("CALL", methodName);

            var tCall = new TypeDefinition(
                                tCont.Namespace,
                                callClassName,
                                TypeAttributes.Class
                                    | TypeAttributes.NestedPrivate
                                    | TypeAttributes.BeforeFieldInit
                                    | TypeAttributes.AutoClass
                                    | TypeAttributes.AnsiClass
                                );

            tCont.NestedTypes.Add(tCall);

            var types = new ScopeTypeMapper(tCall);





            var rtContGenArgs = tContRef is GenericInstanceType
                                    ? ((GenericInstanceType)tContRef).GenericArguments.ToArray()
                                    : new TypeReference[0];

            var rtMethodGenArgs = mOuterRef is GenericInstanceMethod
                                    ? ((GenericInstanceMethod)mOuterRef).GenericArguments.ToArray()
                                    : new TypeReference[0];



            foreach(var tContGenArg in rtContGenArgs) {
                types.Map(tContGenArg);
            }


            var tInstance = isStaticMethod
                                ? null
                                : types.Map(tContRef);

            var tReturn = returnsValue
                                ? types.Map(mOuterRef.ReturnType)
                                : null;

            var tCallBaseRef = R.CallBase_Type.MakeGenericInstanceType(
                                                tInstance ?? mod.TypeSystem.Object,
                                                tReturn ?? mod.TypeSystem.Object
                                                );

            tCall.BaseType = tCallBaseRef;


            foreach(var tMethodGenArg in rtMethodGenArgs) {
                types.Map(tMethodGenArg);
            }






            var args = ArgSpec.CreateAll(_ctx, types, methodArgs);

            var byrefArgs = args.Where(a => a.IsByRef)
                                    .ToArray();


            
            var tCallRef = tCall.HasGenericParameters
                            ? tCall.MakeGenericInstanceType(tCall.GenericParameters.ToArray())
                            : (TypeReference)tCall;

            var fInstance = isStaticMethod
                                ? null
                                : tCallBaseRef.ReferenceField(R.CallBase_fInstance.Name);

            var fReturn = returnsValue
                                ? tCallBaseRef.ReferenceField(R.CallBase_fReturn.Name)
                                : null;

            var fArgs = tCallBaseRef.ReferenceField(R.CallBase_fCallArgs.Name);


            var CallBase_mCtor = tCallBaseRef.ReferenceMethod(m => m.IsConstructor);

            var mCtor = tCall.AddCtor(
                                new[] {
                                    R.Roost_Type,
                                    tContRef,
                                    mod.ImportReference(typeof(ICallArg[]))
                                },
                                (i, m) => {
                                    i.Emit(OpCodes.Ldarg_0);
                                    i.Emit(OpCodes.Ldarg_1);
                                    i.Emit(OpCodes.Ldarg_2);
                                    i.Emit(OpCodes.Ldarg_3);
                                    i.Emit(OpCodes.Call, CallBase_mCtor);
                                    i.Emit(OpCodes.Ret);
                                });


            var CallBase_mDispatchFinal = tCallBaseRef.ReferenceMethod(R.CallBase_mDispatchFinal.Name);

            tCall.OverrideMethod(
                        CallBase_mDispatchFinal,
                        (i, m) => {
                            //load instance to stack (if non-static)
                            //load args onto stack (or address thereof if byref)

                            //call mInner

                            //save return value to fReturn

                            i.Emit(OpCodes.Ret);
                        });

            
            
            //return populated CallInfo
            return new CallInfo(
                            tCall,
                            mCtor,
                            R.CallBase_mPreDispatch,
                            R.CallBase_mDispatch,
                            fReturn,
                            fArgs,
                            args
                            );
        }

    }














    /*
    abstract class _CallWeaver
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

        protected CuckooSpec _cuckoo;

        protected class Arg
        {
            public bool IsByRef { get; set; }
            public int Index { get; set; }
            public ParameterDefinition MethodParam { get; set; }
            public ParameterDefinition PrepareParam { get; set; }
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



        public _CallWeaver(WeaveContext ctx) {
            _ctx = ctx;
        }


        protected abstract void InnerAddElements();
        protected abstract void InnerEmitCtor(ILProcessor il);
        protected abstract void InnerEmitPrepare(ILProcessor il);
        protected abstract void InnerEmitInvoke(ILProcessor il);



        protected TypeReference[] _rtContGenArgs;
        protected TypeReference[] _rtMethodGenArgs;


        public virtual CallInfo Weave(MethodReference mOuterRef) 
        {
            var R = _ctx.RefMap;
            var mod = _ctx.Module;
            _tCont = _ctx.tCont;
            _tContRef = _ctx.tContRef;
            string methodName = _ctx.mOuter.Name;

            string callClassName = _ctx.NameSource.GetElementName("CALL", methodName);

            _tCall = new TypeDefinition(
                                _tCont.Namespace,
                                callClassName,
                                TypeAttributes.Class 
                                    | TypeAttributes.NestedPrivate 
                                    | TypeAttributes.BeforeFieldInit 
                                    | TypeAttributes.AutoClass 
                                    | TypeAttributes.AnsiClass
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

            if(!_ctx.mInner.IsStatic) {
                _fInstance = _tCall.AddField(_tContRef, "_instance");
            }
            
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
                                                PrepareParam = new ParameterDefinition(argType),
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

            if(_fInstance != null) {
                _fInstanceRef = _tCallRef.ReferenceField(_fInstance.Name);
            }

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
                            if(_ctx.mInner.IsStatic) {
                                i.Emit(OpCodes.Ldnull);
                            }
                            else {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, _fInstanceRef);
                            }

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



            var fCuckoo = _tCall.AddField<ICuckoo>("_cuckoo");
            var fCuckooRef = _tCallRef.ReferenceField(fCuckoo.Name);


            InnerAddElements();


            _mCtor = _tCall.AddCtor(
                        (_ctx.mInner.IsStatic
                            ? new[] { 
                                new ParameterDefinition(R.Roost_Type) 
                                }
                            : new[] {
                                new ParameterDefinition(R.Roost_Type),
                                new ParameterDefinition(_tContRef)
                                }),
                        (i, m) => {
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Call, R.Object_mCtor);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldarg_1);
                            i.Emit(OpCodes.Stfld, _fRoostRef);

                            if(!_ctx.mInner.IsStatic) {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldarg_2);
                                i.Emit(OpCodes.Stfld, _fInstanceRef);
                            }

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldarg_1);
                            i.Emit(OpCodes.Callvirt, R.Roost_mGetUsurpers);
                            i.Emit(OpCodes.Ldc_I4, _cuckoo.Index);
                            i.Emit(OpCodes.Ldelem_Ref);
                            i.Emit(OpCodes.Stfld, fCuckooRef);

                            InnerEmitCtor(i);

                            i.Emit(OpCodes.Ret);
                        });



            var mPrepare = _tCall.AddMethod(
                                        "Prepare",
                                        _args.Select(a => a.PrepareParam)
                                                .ToArray(),
                                        mod.TypeSystem.Void,
                                        (i, m) => {
                                            foreach(var arg in _args) {
                                                i.Emit(OpCodes.Ldarg_0);
                                                i.Emit(OpCodes.Ldarg_S, arg.PrepareParam);
                                                i.Emit(OpCodes.Stfld, arg.FieldRef);
                                            }

                                            //could also ensure CallArgs field is null?

                                            //Pass to ICuckoo.OnBeforeCall
                                            //which will come back via ICall.InnerProceed
                                            //...

                                            //Changes to args should be propagated back
                                            //to interior fields...

                                            //Or should there be interior fields at all?
                                            //Shouldn't dispatcher create arg array and pass it on?
                                            //This may be faster than fields - args should perhaps be structs, then
                                            //Then there'd be no need for copying.

                                            InnerEmitPrepare(i);

                                            i.Emit(OpCodes.Ret);
                                        });


            var mInvoke = _tCall.AddMethod(
                                        "Invoke",
                                        new ParameterDefinition[0],
                                        mod.TypeSystem.Void,
                                        (i, m) => {
                      
                                            i.Emit(OpCodes.Ldarg_0);
                                            i.Emit(OpCodes.Ldfld, fCuckooRef);
                                            i.Emit(OpCodes.Ldarg_0);
                                            i.Emit(OpCodes.Callvirt, R.ICuckoo_mOnCall);
                                            

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


                            InnerEmitInvoke(i);


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


                            //if(_fReturn != null) {
                            //    var vReturn = m.Body.AddVariable(_fReturnRef.FieldType);

                            //    i.Emit(OpCodes.Stloc, vReturn);

                            //    i.Emit(OpCodes.Ldarg_0);
                            //    i.Emit(OpCodes.Ldloc, vReturn);
                            //    i.Emit(OpCodes.Stfld, _fReturnRef);
                            //}

                            i.Emit(OpCodes.Ret);
                        });


            return new CallInfo(_tCall,
                                    _mCtor,
                                    mPrepare,
                                    mInvoke,
                                    _fReturn,
                                    _args.Select(a => new CallArgInfo(
                                                            a.Field,
                                                            a.MethodParam,
                                                            a.PrepareParam
                                                            ))
                                    );
        }
    }

    
    class MediateCallWeaver : _CallWeaver
    {
        CallInfo _nextCall;
        FieldReference _fNextCallRef;

        public MediateCallWeaver(WeaveContext ctx, CuckooSpec cuckoo, CallInfo nextCall)
            : base(ctx, cuckoo) 
        {
            _nextCall = nextCall;
        }

        protected override void InnerAddElements() 
        {
            if(_nextCall.RequiresInstanciation) {
                _nextCall = _nextCall.Instanciate(((GenericInstanceType)_tCallRef).GenericArguments);
            }

            var fNextCall = _tCall.AddField(_nextCall.Type, "_nextCall");
            _fNextCallRef = _tCallRef.ReferenceField(fNextCall.Name);
        }

        protected override void InnerEmitCtor(ILProcessor i) {
            i.Emit(OpCodes.Ldarg_0);

            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fRoostRef);

            if(!_ctx.mInner.IsStatic) {
                i.Emit(OpCodes.Ldarg_0);
                i.Emit(OpCodes.Ldfld, _fInstanceRef);
            }

            i.Emit(OpCodes.Newobj, _nextCall.CtorMethod);

            i.Emit(OpCodes.Stfld, _fNextCallRef);
        }

        protected override void InnerEmitPrepare(ILProcessor i) 
        {
            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fNextCallRef);

            foreach(var arg in _args) {
                i.Emit(OpCodes.Ldarg_0);
                i.Emit(OpCodes.Ldfld, arg.FieldRef);
            }

            i.Emit(OpCodes.Call, _nextCall.PreDispatchMethod);

            //copy events back here?
            //....
        }

        protected override void InnerEmitInvoke(ILProcessor i) 
        {
            var R = _ctx.RefMap;
            
            var vCall = i.Body.AddVariable(_nextCall.Type);

            i.Emit(OpCodes.Ldarg_0);
            i.Emit(OpCodes.Ldfld, _fNextCallRef);
            i.Emit(OpCodes.Stloc_S, vCall);

            i.Emit(OpCodes.Ldloc_S, vCall);
            i.Emit(OpCodes.Call, _nextCall.DispatchMethod);

            foreach(var nextCallArg in _nextCall.Args.Where(a => a.IsByRef)) {
                i.Emit(OpCodes.Ldarg_0);
                i.Emit(OpCodes.Ldloc_S, vCall);
                i.Emit(OpCodes.Ldfld, nextCallArg.Field);
                i.Emit(OpCodes.Stfld, _args.First(a => a.Field.Name == nextCallArg.Field.Name).FieldRef); //!!!!!!!!!!!!!!!!!!
            }

            if(_nextCall.ReturnsValue) {
                i.Emit(OpCodes.Ldarg_0);
                i.Emit(OpCodes.Ldloc, vCall);
                i.Emit(OpCodes.Ldfld, _nextCall.ReturnField);
                i.Emit(OpCodes.Stfld, _fReturnRef);
            }

        }
    }


    class FinalCallWeaver : _CallWeaver
    {
        MethodDefinition _mInner;

        public FinalCallWeaver(WeaveContext ctx, CuckooSpec cuckoo, MethodDefinition mInner)
            : base(ctx, cuckoo) 
        {
            _mInner = mInner;
        }

        protected override void InnerAddElements() {
            //...
        }

        protected override void InnerEmitCtor(ILProcessor il) {
            //...
        }

        protected override void InnerEmitPrepare(ILProcessor il) {
            //...
        }

        protected override void InnerEmitInvoke(ILProcessor i) 
        {
            var mInnerRef = _mInner.CloneWithNewDeclaringType(_tContRef);

            if(mInnerRef.HasGenericParameters) {
                mInnerRef = mInnerRef.MakeGenericInstanceMethod(
                                        ((GenericInstanceType)_tCallRef).GenericArguments
                                                                            .Skip(_rtContGenArgs.Length)
                                                                            .ToArray());
            }
        
            //don't do this if no instance...
            //in fact, should get shot of instance field completely if static

            if(!_mInner.IsStatic) {
                i.Emit(OpCodes.Ldarg_0);
                i.Emit(OpCodes.Ldfld, _fInstanceRef);
            }

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

            //arg fields should be automatically updated

            if(mInnerRef.ReturnsValue()) {
                var vReturn = i.Body.AddVariable(_fReturnRef.FieldType);

                i.Emit(OpCodes.Stloc_S, vReturn);
                i.Emit(OpCodes.Ldarg_0);
                i.Emit(OpCodes.Ldloc_S, vReturn);
                i.Emit(OpCodes.Stfld, _fReturnRef);
            }
        }
    }
    */
}
