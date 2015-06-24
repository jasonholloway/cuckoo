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


            //gen 


            return baseType;
        } 

    }
}
