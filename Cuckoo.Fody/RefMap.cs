using Cuckoo.Common;
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
    using Refl = System.Reflection;

    class RefMap
    {
        public readonly TypeReference ICuckoo_TypeRef;
        public readonly TypeReference Roost_TypeRef;
        public readonly TypeReference CallArg_TypeRef;
        public readonly TypeReference ICallArgStatus_TypeRef;
        public readonly TypeReference ICall_TypeRef;
        public readonly TypeReference MethodInfo_TypeRef;

        public readonly MethodReference CallArg_mCtor;
        public readonly MethodReference CallArg_mGetValue;
        public readonly MethodReference CallArg_mSetValue;
        public readonly MethodReference ICallArgStatus_mGetHasChanged;
        public readonly MethodReference Roost_mCtor;
        public readonly MethodReference Roost_mGetParams;
        public readonly MethodReference Roost_mGetUsurpers;
        public readonly MethodReference ICuckoo_mInit;
        public readonly MethodReference ICuckoo_mUsurp;
        public readonly MethodReference CuckooedAtt_mCtor;
        public readonly MethodReference MethodInfo_mGetMethodFromHandle;
        public readonly MethodReference Object_mCtor;

        public RefMap(ModuleDefinition module, MethodDefinition method) {
            ICuckoo_TypeRef = module.ImportReference(typeof(ICuckoo));
            Roost_TypeRef = module.ImportReference(typeof(Roost));
            CallArg_TypeRef = module.ImportReference(typeof(CallArg));
            ICallArgStatus_TypeRef = module.ImportReference(typeof(ICallArgStatus));
            ICall_TypeRef = module.ImportReference(typeof(ICall));
            MethodInfo_TypeRef = module.ImportReference(typeof(Refl.MethodInfo));

            CallArg_mCtor = module.ImportReference(
                                        CallArg_TypeRef.Resolve().GetConstructors().First());

            ICallArgStatus_mGetHasChanged = module.ImportReference(
                                                ICallArgStatus_TypeRef.Resolve().GetMethod("get_HasChanged"));

            CallArg_mGetValue = module.ImportReference(
                                        CallArg_TypeRef.GetMethod("get_Value"));

            CallArg_mSetValue = module.ImportReference(
                                        CallArg_TypeRef.GetMethod("set_Value"));

            Roost_mGetParams = module.ImportReference(
                                        Roost_TypeRef.GetMethod("get_Parameters"));

            Roost_mGetUsurpers = module.ImportReference(
                                        Roost_TypeRef.GetMethod("get_Cuckoos"));

            Roost_mCtor = module.ImportReference(
                                        Roost_TypeRef.Resolve().GetConstructors().First());

            ICuckoo_mInit = module.ImportReference(
                                        typeof(ICuckoo).GetMethod("Init"));

            ICuckoo_mUsurp = module.ImportReference(
                                        typeof(ICuckoo).GetMethod("Usurp"));

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
