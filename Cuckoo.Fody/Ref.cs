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

    class Ref
    {
        public readonly TypeReference ICallUsurper_TypeRef;
        public readonly TypeReference CallSite_TypeRef;
        public readonly TypeReference CallArg_TypeRef;
        public readonly TypeReference ICall_TypeRef;
        public readonly TypeReference MethodInfo_TypeRef;

        public readonly MethodReference CallArg_mCtor;
        public readonly MethodReference CallArg_mGetIsPristine;
        public readonly MethodReference CallArg_mGetValue;
        public readonly MethodReference CallSite_mGetParams;
        public readonly MethodReference CallSite_mGetUsurpers;
        public readonly MethodReference CallSite_mCtor;
        public readonly MethodReference ICallUsurper_mInit;
        public readonly MethodReference ICallUsurper_mUsurp;
        public readonly MethodReference UsurpedAtt_mCtor;
        public readonly MethodReference MethodInfo_mGetMethodFromHandle;

        public Ref(ModuleDefinition module, MethodDefinition method) {
            ICallUsurper_TypeRef = module.ImportReference(typeof(ICallUsurper));
            CallSite_TypeRef = module.ImportReference(typeof(Cuckoo.Common.CallSite));
            CallArg_TypeRef = module.ImportReference(typeof(CallArg));
            ICall_TypeRef = module.ImportReference(typeof(ICall));
            MethodInfo_TypeRef = module.ImportReference(typeof(Refl.MethodInfo));

            CallArg_mCtor = module.ImportReference(
                                        CallArg_TypeRef.Resolve().GetConstructors().First());

            CallArg_mGetIsPristine = module.ImportReference(
                                        CallArg_TypeRef.GetMethod("get_IsPristine"));

            CallArg_mGetValue = module.ImportReference(
                                        CallArg_TypeRef.GetMethod("get_Value"));

            CallSite_mGetParams = module.ImportReference(
                                        CallSite_TypeRef.GetMethod("get_Parameters"));

            CallSite_mGetUsurpers = module.ImportReference(
                                        CallSite_TypeRef.GetMethod("get_Usurpers"));

            CallSite_mCtor = module.ImportReference(
                                        CallSite_TypeRef.Resolve().GetConstructors().First());

            ICallUsurper_mInit = module.ImportReference(
                                        typeof(ICallUsurper).GetMethod("Init"));

            ICallUsurper_mUsurp = module.ImportReference(
                                        typeof(ICallUsurper).GetMethod("Usurp"));

            UsurpedAtt_mCtor = module.ImportReference(
                                        typeof(UsurpedAttribute).GetConstructor(new[] { typeof(string) }));

            MethodInfo_mGetMethodFromHandle =
                    module.ImportReference(typeof(Refl.MethodBase)
                                                .GetMethod(
                                                    "GetMethodFromHandle",
                                                    Refl.BindingFlags.Static
                                                        | Refl.BindingFlags.Public,
                                                    null,
                                                    new[] { typeof(RuntimeMethodHandle) },
                                                    null
                                                    )
                                                );


        }
    }

}
