using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Fody.Cecil
{
    internal static class TypeReferenceExtensions
    {
        public static bool ImplementsInterface<TInterface>(this TypeReference @this) {
            return @this.ImplementsInterface(typeof(TInterface).FullName);
        }

        public static bool ImplementsInterface(this TypeReference @this, string interfaceName) {
            var typeDef = @this.Resolve();

            return (typeDef.HasInterfaces
                            && typeDef.Interfaces.Any(t => t.FullName == interfaceName))
                    || (typeDef.BaseType != null
                            && typeDef.BaseType.ImplementsInterface(interfaceName));

        }

    }
}
