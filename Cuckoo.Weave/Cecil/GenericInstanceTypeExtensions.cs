using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Linq;

namespace Cuckoo.Weave.Cecil
{
    internal static class GenericInstanceTypeExtensions
    {

        public static TypeReference GetBaseType(this GenericInstanceType @this) {
            var baseType = @this.Resolve().BaseType;

            if(baseType is GenericInstanceType) {
                var baseInst = (GenericInstanceType)baseType;

                var tups = @this.GetElementType().GenericParameters
                                    .Zip(@this.GenericArguments,
                                                    (p, a) => new {
                                                        Param = p,
                                                        Arg = a
                                                    }).ToArray();

                var origArgs = baseInst.GenericArguments.ToArray();
                
                var newArgs = origArgs.Select(a => {
                    var tup = tups.FirstOrDefault(t => t.Param == a);
                    return tup != null ? tup.Arg : a;
                });                                                     

                baseType = baseType.GetElementType()
                                .MakeGenericInstanceType(newArgs.ToArray());
            }

            return baseType;
        } 

    }
}
