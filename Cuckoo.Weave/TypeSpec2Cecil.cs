using Cuckoo.Gather.Specs;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Weave
{
    public class TypeSpec2Cecil
    {
        ModuleDefinition _mod;

        public TypeSpec2Cecil(ModuleDefinition mod) {
            _mod = mod;
        }

        public TypeReference Resolve(TypeSpec typeSpec) 
        {
            //get assembly
            var asm = _mod.AssemblyResolver.Resolve(typeSpec.AssemblyName);

            var typeRef = (TypeReference)asm.MainModule.GetType(typeSpec.FullName);

            typeRef = _mod.Import(typeRef);
            
            var genTypeRefs = typeSpec.GenArgs
                                        .Select(a => Resolve(a))
                                        .ToArray();
            
            if(genTypeRefs.Any()) {
                typeRef = (TypeReference)typeRef.MakeGenericInstanceType(genTypeRefs);
            }

            return typeRef;
        }

    }
}
