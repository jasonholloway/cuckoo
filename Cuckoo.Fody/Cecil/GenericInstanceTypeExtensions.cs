using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody.Cecil
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

                baseInst.GenericArguments.Clear();

                var newArgs = origArgs.Select(a => {
                    var tup = tups.FirstOrDefault(t => t.Param == a);
                    return tup != null ? tup.Arg : a ; //.Param : a;
                });

                foreach(var newArg in newArgs) {
                    baseInst.GenericArguments.Add(newArg);
                }
            }

            return baseType;
        } 

    }
}
