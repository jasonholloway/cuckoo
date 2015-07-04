using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Weave.Cecil
{
    public static class ModuleDefinitionExtensions
    {

        public static IEnumerable<TypeDefinition> GetAllTypes(this ModuleDefinition mod) {
            return mod.Types.SelectMany(t => t.GetAllTypes());
        }

    }
}
