﻿using Cuckoo.Common;
using Cuckoo.Fody.Cecil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Linq;

namespace Cuckoo.Fody
{
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

        }

        protected Arg[] _args;
        protected Arg[] _byrefArgs;



        public CallWeaver(WeaveContext ctx) {
            _ctx = ctx;
        }



        //arg specs too, please!
        //...





        protected abstract void EmitInnerInvoke(ILProcessor il, MethodReference mOuterRef);



        protected TypeReference[] _rtContGenArgs;
        protected TypeReference[] _rtMethodGenArgs;


        public CallInfo Weave(MethodReference mOuterRef) {
            var R = _ctx.RefMap;
            var mod = _ctx.Module;
            //var mOuter = _ctx.OuterMethod;
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
            _tCall.Interfaces.Add(R.ICall_TypeRef);



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

                                    return new Arg() {
                                                Index = iArg++,
                                                MethodParam = p,
                                                CtorParam = new ParameterDefinition(argType),
                                                IsByRef = p.ParameterType.IsByReference,
                                                Field = _tCall.AddField(argType,
                                                                            "_arg_" + p.Name,
                                                                            FieldAttributes.Public )
                                            };
                                }) 
                                .ToArray();

            _byrefArgs = _args.Where(a => a.IsByRef)
                                .ToArray();


            _tCallRef = _tCall.HasGenericParameters
                        ? _tCall.MakeGenericInstanceType(_tCall.GenericParameters.ToArray()) //contGenArgs.Concat(methodGenArgs).ToArray())
                        : (TypeReference)_tCall;
            
            _fInstanceRef = _tCallRef.ReferenceField(_fInstance.Name);

            _fRoostRef = _tCallRef.ReferenceField(_fRoost.Name);

            if(_fReturn != null) {
                _fReturnRef = _tCallRef.ReferenceField(_fReturn.Name);
            }

            foreach(var arg in _args) {
                arg.FieldRef = _tCallRef.ReferenceField(arg.Field.Name);
            }



            _mCtor = _tCall.AddCtor(
                        new[] {
                            new ParameterDefinition(R.Roost_TypeRef),
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
            
            _mCtorRef = _tCallRef.ReferenceMethod(c => c.IsConstructor);


            _tCall.OverrideMethod(
                        "get_Instance",
                        R.ICall_TypeRef,
                        (i, m) => {
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, _fInstanceRef);
                            i.Emit(OpCodes.Ret);
                        });

            _tCall.OverrideMethod(
                        "get_Method",
                        R.ICall_TypeRef,
                        (i, m) => {
                            var Roost_mGetMethod = mod.ImportReference(
                                                                R.Roost_TypeRef.Resolve().GetMethod("get_Method"));
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, _fRoostRef);
                            i.Emit(OpCodes.Call, Roost_mGetMethod);
                            i.Emit(OpCodes.Ret);
                        });


            var fCallArgs = _tCall.AddField<CallArg[]>("_rCallArgs");
            var fCallArgsRef = _tCallRef.ReferenceField(f => f.Name == fCallArgs.Name);

            _tCall.OverrideMethod(
                        "get_Args",
                        R.ICall_TypeRef,
                        (i, m) => {
                            var vArgs = m.Body.AddVariable<CallArg[]>();
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
                            i.Emit(OpCodes.Newarr, R.CallArg_TypeRef);
                            i.Emit(OpCodes.Stloc_S, vArgs);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, _fRoostRef);
                            i.Emit(OpCodes.Call, R.Roost_mGetParams);
                            i.Emit(OpCodes.Stloc_S, vParams);

                            for(int iA = 0; iA < _args.Length; iA++) {
                                i.Emit(OpCodes.Ldloc_S, vArgs);
                                i.Emit(OpCodes.Ldc_I4, iA);

                                //load parameter
                                i.Emit(OpCodes.Ldloc_S, vParams);
                                i.Emit(OpCodes.Ldc_I4, iA);
                                i.Emit(OpCodes.Ldelem_Ref);

                                //load value & box
                                var arg = _args[iA];
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, arg.FieldRef);
                                i.Emit(OpCodes.Box, arg.FieldType);

                                i.Emit(OpCodes.Newobj, R.CallArg_mCtor); //don't we need to say whether CallArg is byref?

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
                        R.ICall_TypeRef,
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
                        R.ICall_TypeRef,
                        (i, m) => {
                            if(_fReturn != null) {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldarg_1);
                                i.Emit(OpCodes.Unbox_Any, _fReturnRef.FieldType);
                                i.Emit(OpCodes.Stfld, _fReturnRef);
                            }

                            i.Emit(OpCodes.Ret);
                        });


            var mPostUsurp = _tCall.AddMethod(
                                        "PostUsurp",
                                        new ParameterDefinition[0],
                                        mod.TypeSystem.Void,
                                        (i, m) => {         
                                            var lbSkipArgFieldUpdate = i.Create(OpCodes.Nop);

                                            i.Emit(OpCodes.Ldarg_0);
                                            i.Emit(OpCodes.Ldfld, fCallArgsRef);
                                            i.Emit(OpCodes.Ldnull);
                                            i.Emit(OpCodes.Ceq);
                                            i.Emit(OpCodes.Brtrue, lbSkipArgFieldUpdate);

                                            //All ByRef Arg objects to have values copied to passed address
                                            //...

                                            foreach(var arg in _byrefArgs) {
                                                //use index to key into created arg object array
                                                //...
                                            }

                                            i.Append(lbSkipArgFieldUpdate);
                                            i.Emit(OpCodes.Ret);
                                        });




            _tCall.OverrideMethod(
                        "CallInner",
                        R.ICall_TypeRef,
                        (i, m) => {
                            ///////////////////////////////////////////////////////////////
                            //Update arg fields if args not pristine
                            var vArgs = i.Body.AddVariable<CallArg[]>();
                            var vArg = i.Body.AddVariable<CallArg>();
                            var lbSkipAllArgUpdates = i.Create(OpCodes.Nop);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCallArgsRef);
                            i.Emit(OpCodes.Stloc_S, vArgs);

                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ldnull);
                            i.Emit(OpCodes.Ceq);
                            i.Emit(OpCodes.Brtrue, lbSkipAllArgUpdates);

                            int iA = 0;

                            foreach(var arg in _args) {
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
                                i.Emit(OpCodes.Unbox_Any, arg.FieldType);

                                i.Emit(OpCodes.Stfld, arg.FieldRef);

                                //should reset pristine flag here
                                //...

                                i.Append(lbSkipArgUpdate);

                                iA++;
                            }

                            i.Append(lbSkipAllArgUpdates);

                            //Subclass loads args and calls inner

                            EmitInnerInvoke(i, mOuterRef);

                            if(_fReturn != null) {
                                var vReturn = m.Body.AddVariable(_fReturnRef.FieldType);

                                i.Emit(OpCodes.Stloc, vReturn);

                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldloc, vReturn);
                                i.Emit(OpCodes.Stfld, _fReturnRef);
                            }


                            //**********************************************************************
                            //ByRef arg fields are copied over to their respective CallArg objects
                            //so as to expose changes to prevailing Cuckoo
                            //...
                            //**********************************************************************


                            i.Emit(OpCodes.Ret);
                        });


            return new CallInfo(_tCall,
                                    _mCtor,
                                    mPostUsurp,
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
            i.Emit(OpCodes.Callvirt, R.ICuckoo_mUsurp);

            i.Emit(OpCodes.Ldloc, vCall);
            i.Emit(OpCodes.Call, nextCall.PostUsurpMethod);

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
                       
            //Our byref arg fields should be sorted

            //No mPostUsurp of course - need to cover for this HERE
            //...
        }
    }

}
