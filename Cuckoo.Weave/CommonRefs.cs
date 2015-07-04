using Cuckoo.Weave.Cecil;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;
using Cuckoo.Impl;
using System.Diagnostics;

namespace Cuckoo.Weave
{
    using Refl = System.Reflection;

    class CommonRefs
    {
        public readonly TypeReference MethodInfo_Type;
        public readonly TypeReference ParamInfo_Type;

        public readonly TypeReference ICallArg_Type;
        public readonly TypeReference CallArg_Type;
        public readonly MethodReference CallArg_mCtor;
        public readonly FieldReference CallArg_fValue;

        public readonly TypeReference ICall_Type;
        public readonly TypeReference CallBase_Type;
        public readonly MethodReference CallBase_mPrepare;
        public readonly MethodReference CallBase_mInvoke;
        public readonly MethodReference CallBase_mInvokeFinal;
        public readonly FieldReference CallBase_fInstance;
        public readonly FieldReference CallBase_fReturn;
        public readonly FieldReference CallBase_fCallArgs;
        public readonly FieldReference CallBase_fArgFlags;

        public readonly TypeReference Roost_Type;
        public readonly MethodReference Roost_mCtor;
        public readonly MethodReference Roost_mInit;
        public readonly MethodReference Roost_mGetParams;
        public readonly MethodReference Roost_mGetCuckoos;

        public readonly TypeReference ICuckooProvider_Type;

        public readonly TypeReference ICuckoo_Type;
        public readonly MethodReference ICuckoo_mInit;
        public readonly MethodReference ICuckoo_mPreCall;
        public readonly MethodReference ICuckoo_mCall;
        
        public readonly MethodReference CuckooedAtt_mCtor;

        public readonly MethodReference DebuggerHiddenAtt_mCtor;

        public readonly MethodReference MethodBase_mGetMethodFromHandle;
        public readonly MethodReference Object_mCtor;

        public CommonRefs(ModuleDefinition module, MethodDefinition method) 
        {
            MethodInfo_Type = module.Import(typeof(Refl.MethodInfo));
            ParamInfo_Type = module.Import(typeof(Refl.ParameterInfo));

            ICall_Type = module.Import(typeof(ICall));
            CallBase_Type = module.Import(typeof(CallBase<,>));
            ICallArg_Type = module.Import(typeof(ICallArg));

            ICuckooProvider_Type = module.Import(
                                            typeof(ICuckooProvider));

            ICuckoo_Type = module.Import(
                                            typeof(ICuckoo));

            ICuckoo_mInit = module.Import(
                                        typeof(ICuckoo).GetMethod("Init"));

            ICuckoo_mPreCall = module.Import(
                                        typeof(ICuckoo).GetMethod("PreCall"));

            ICuckoo_mCall = module.Import(
                                        typeof(ICuckoo).GetMethod("Call"));

            CallBase_mPrepare = module.Import(
                                        CallBase_Type.GetMethod("Prepare"));

            CallBase_mInvoke = module.Import(
                                        CallBase_Type.GetMethod("Invoke"));

            CallBase_mInvokeFinal = module.Import(
                                        CallBase_Type.GetMethod("InvokeFinal"));

            CallBase_fInstance = module.Import(
                                        CallBase_Type.GetField("_instance"));

            CallBase_fReturn = module.Import(
                                        CallBase_Type.GetField("_return"));

            CallBase_fCallArgs = module.Import(
                                        CallBase_Type.GetField("_callArgs"));

            CallBase_fArgFlags = module.Import(
                                        CallBase_Type.GetField("_argFlags"));


            Roost_Type = module.Import(typeof(Roost));

            Roost_mCtor = module.Import(
                                        Roost_Type.Resolve().GetConstructors().First());

            Roost_mInit = module.Import(
                                        Roost_Type.Resolve().GetMethod("Init"));

            Roost_mGetParams = module.Import(
                                        Roost_Type.GetMethod("get_Parameters"));

            Roost_mGetCuckoos = module.Import(
                                        Roost_Type.GetMethod("get_Cuckoos"));



            CallArg_Type = module.Import(typeof(CallArg<>));

            CallArg_mCtor = module.Import(
                                        CallArg_Type.Resolve().GetConstructors().First());

            CallArg_fValue = module.Import(
                                        CallArg_Type.GetField("_value"));



            CuckooedAtt_mCtor = module.Import(
                                        typeof(CuckooedAttribute).GetConstructor(new[] { typeof(string) }));

            DebuggerHiddenAtt_mCtor = module.Import(
                                        typeof(DebuggerHiddenAttribute).GetConstructor(Type.EmptyTypes));

            MethodBase_mGetMethodFromHandle =
                    module.Import(typeof(Refl.MethodBase)
                                                .GetMethod(
                                                    "GetMethodFromHandle",
                                                    Refl.BindingFlags.Static
                                                        | Refl.BindingFlags.Public,
                                                    null,
                                                    new[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) },
                                                    null
                                                    )
                                                );
            
            Object_mCtor = module.Import(
                                    module.TypeSystem.Object.ReferenceMethod(m => m.IsConstructor));
        }

    }

}
