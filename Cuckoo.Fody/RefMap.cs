using Cuckoo;
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
        public readonly TypeReference ICuckoo_Type;
        public readonly TypeReference Roost_Type;
        public readonly TypeReference ICall_Type;
        public readonly TypeReference ICallArg_Type;
        public readonly TypeReference CallArg_Type;
        public readonly TypeReference MethodInfo_Type;
        public readonly TypeReference ParamInfo_Type;

        public readonly MethodReference CallArg_mCtor;
        public readonly MethodReference CallArg_mGetTypedValue;
        public readonly MethodReference CallArg_mSetTypedValue;
        public readonly MethodReference CallArg_mGetHasChanged;

        public readonly MethodReference Roost_mCtor;
        public readonly MethodReference Roost_mGetParams;
        public readonly MethodReference Roost_mGetUsurpers;
        public readonly MethodReference ICuckoo_mOnRoost;
        public readonly MethodReference ICuckoo_mOnCall;
        public readonly MethodReference CuckooedAtt_mCtor;
        public readonly MethodReference MethodInfo_mGetMethodFromHandle;
        public readonly MethodReference Object_mCtor;

        public RefMap(ModuleDefinition module, MethodDefinition method) 
        {
            MethodInfo_Type = module.ImportReference(typeof(Refl.MethodInfo));
            ParamInfo_Type = module.ImportReference(typeof(Refl.ParameterInfo));

            ICuckoo_Type = module.ImportReference(typeof(ICuckoo));
            Roost_Type = module.ImportReference(typeof(Roost));
            ICall_Type = module.ImportReference(typeof(ICall));
            ICallArg_Type = module.ImportReference(typeof(ICallArg));
            CallArg_Type = module.ImportReference(typeof(CallArg<>));


            ICuckoo_mOnRoost = module.ImportReference(
                                        typeof(ICuckoo).GetMethod("OnRoost"));

            ICuckoo_mOnCall = module.ImportReference(
                                        typeof(ICuckoo).GetMethod("OnCall"));


            Roost_mCtor = module.ImportReference(
                                        Roost_Type.Resolve().GetConstructors().First());

            Roost_mGetParams = module.ImportReference(
                                        Roost_Type.GetMethod("get_Parameters"));

            Roost_mGetUsurpers = module.ImportReference(
                                        Roost_Type.GetMethod("get_Cuckoos"));


            CallArg_mCtor = module.ImportReference(
                                        CallArg_Type.Resolve().GetConstructors().First());

            CallArg_mGetHasChanged = module.ImportReference(
                                        CallArg_Type.Resolve().GetMethod("get_HasChanged"));

            CallArg_mGetTypedValue = module.ImportReference(
                                        CallArg_Type.GetMethod("get_TypedValue"));

            CallArg_mSetTypedValue = module.ImportReference(
                                        CallArg_Type.GetMethod("set_TypedValue"));


            CuckooedAtt_mCtor = module.ImportReference(
                                        typeof(CuckooedAttribute).GetConstructor(new[] { typeof(string) }));

            MethodInfo_mGetMethodFromHandle =
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
