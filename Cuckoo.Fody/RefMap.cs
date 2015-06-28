﻿using Cuckoo;
using Cuckoo.Fody.Cecil;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    using Cuckoo.Attributes;
    using Cuckoo.Impl;
    using Refl = System.Reflection;

    class RefMap
    {
        public readonly TypeReference MethodInfo_Type;
        public readonly TypeReference ParamInfo_Type;

        public readonly TypeReference ICallArg_Type;
        public readonly TypeReference CallArg_Type;
        public readonly MethodReference CallArg_mCtor;
        public readonly FieldReference CallArg_fValue;

        public readonly TypeReference ICall_Type;
        public readonly TypeReference CallBase_Type;
        public readonly MethodReference CallBase_mPreInvoke;
        public readonly MethodReference CallBase_mInvokeNext;
        public readonly MethodReference CallBase_mInvokeFinal;
        public readonly FieldReference CallBase_fInstance;
        public readonly FieldReference CallBase_fReturn;
        public readonly FieldReference CallBase_fCallArgs;

        public readonly TypeReference Roost_Type;
        public readonly MethodReference Roost_mCtor;
        public readonly MethodReference Roost_mGetParams;
        public readonly MethodReference Roost_mGetCuckoos;

        public readonly TypeReference ICuckoo_Type;
        public readonly MethodReference ICuckoo_mInit;
        public readonly MethodReference ICuckoo_mPreCall;
        public readonly MethodReference ICuckoo_mCall;
        
        public readonly MethodReference CuckooedAtt_mCtor;
        public readonly MethodReference MethodBase_mGetMethodFromHandle;
        public readonly MethodReference Object_mCtor;

        public RefMap(ModuleDefinition module, MethodDefinition method) 
        {
            MethodInfo_Type = module.ImportReference(typeof(Refl.MethodInfo));
            ParamInfo_Type = module.ImportReference(typeof(Refl.ParameterInfo));

            ICuckoo_Type = module.ImportReference(typeof(ICuckoo));
            ICall_Type = module.ImportReference(typeof(ICall));
            CallBase_Type = module.ImportReference(typeof(CallBase<,>));
            ICallArg_Type = module.ImportReference(typeof(ICallArg));


            ICuckoo_mInit = module.ImportReference(
                                        typeof(ICuckoo).GetMethod("Init"));

            ICuckoo_mPreCall = module.ImportReference(
                                        typeof(ICuckoo).GetMethod("PreCall"));

            ICuckoo_mCall = module.ImportReference(
                                        typeof(ICuckoo).GetMethod("Call"));

            CallBase_mPreInvoke = module.ImportReference(
                                        CallBase_Type.GetMethod("PreInvoke"));

            CallBase_mInvokeNext = module.ImportReference(
                                        CallBase_Type.GetMethod("InvokeNext"));

            CallBase_mInvokeFinal = module.ImportReference(
                                        CallBase_Type.GetMethod("InvokeFinal"));

            CallBase_fInstance = module.ImportReference(
                                        CallBase_Type.GetField("_instance"));

            CallBase_fReturn = module.ImportReference(
                                        CallBase_Type.GetField("_return"));

            CallBase_fCallArgs = module.ImportReference(
                                        CallBase_Type.GetField("_callArgs"));



            Roost_Type = module.ImportReference(typeof(Roost));

            Roost_mCtor = module.ImportReference(
                                        Roost_Type.Resolve().GetConstructors().First());

            Roost_mGetParams = module.ImportReference(
                                        Roost_Type.GetMethod("get_Parameters"));

            Roost_mGetCuckoos = module.ImportReference(
                                        Roost_Type.GetMethod("get_Cuckoos"));



            CallArg_Type = module.ImportReference(typeof(CallArg<>));

            CallArg_mCtor = module.ImportReference(
                                        CallArg_Type.Resolve().GetConstructors().First());

            CallArg_fValue = module.ImportReference(
                                        CallArg_Type.GetField("_value"));



            CuckooedAtt_mCtor = module.ImportReference(
                                        typeof(CuckooedAttribute).GetConstructor(new[] { typeof(string) }));

            MethodBase_mGetMethodFromHandle =
                    module.ImportReference(typeof(Refl.MethodBase)
                                                .GetMethod(
                                                    "GetMethodFromHandle",
                                                    Refl.BindingFlags.Static
                                                        | Refl.BindingFlags.Public,
                                                    null,
                                                    new[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) },
                                                    null
                                                    )
                                                );
            
            Object_mCtor = module.ImportReference(
                                    module.TypeSystem.Object.ReferenceMethod(m => m.IsConstructor));
        }

    }

}
