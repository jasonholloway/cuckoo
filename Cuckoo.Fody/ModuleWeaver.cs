using Cuckoo;
using Cuckoo.Attributes;
using Mono.Cecil;
using Mono.Cecil.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    public class ModuleWeaver
    {
        public ModuleDefinition ModuleDefinition { get; set; }
        public Action<string> LogInfo { get; set; }
                       

        public void Execute() {
            var commonModule = ModuleDefinition.ReadModule("Cuckoo.dll");

            var weaveSpecs = GetAllTypes(this.ModuleDefinition)
                                .SelectMany(t => t.Methods)
                                    .Where(m => m.HasCustomAttributes && !m.IsAbstract)
                                    .Select(m => new WeaveSpec() {
                                                            Method = m,
                                                            CuckooAttributes = m.CustomAttributes
                                                                                .Where(a => IsCuckooAttributeType(a.AttributeType))
                                                                                .Reverse()
                                                                                .ToArray()
                                                        })
                                                        .Where(spec => spec.CuckooAttributes.Any());

            var weaves = weaveSpecs
                            .Select(spec => new Weaver(spec, LogInfo))
                            .ToArray(); //needed 

            foreach(var weave in weaves) {
                weave.Weave();
            }
        }



        IEnumerable<TypeDefinition> GetAllTypes(TypeDefinition type) {
            yield return type;

            if(type.HasNestedTypes) {
                foreach(var nestedType in type.NestedTypes) {
                    foreach(var t in GetAllTypes(nestedType)) {
                        yield return t;
                    }
                }
            }
        }

        IEnumerable<TypeDefinition> GetAllTypes(ModuleDefinition mod) {
            return mod.Types.SelectMany(t => GetAllTypes(t));
        }


        bool IsCuckooAttributeType(TypeReference typeRef) {
            if(typeRef.FullName == typeof(CuckooAttribute).FullName) {
                return true;
            }
            else {
                var baseType = typeRef.Resolve().BaseType;

                return baseType != null
                            ? IsCuckooAttributeType(baseType)
                            : false;
            }
        }

    }
}
