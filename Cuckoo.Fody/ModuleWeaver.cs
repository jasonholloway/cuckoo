using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cuckoo.Fody
{
    public class ModuleWeaver
    {
        public ModuleDefinition ModuleDefinition { get; set; }
        public Action<string> LogInfo { get; set; }

        public void Execute() {


            if(ModuleDefinition == null) {
                throw new InvalidOperationException("MethodDefinition is strangely null!");
            }


            var weaveSpecs = GetAllTypes(this.ModuleDefinition)
                                .SelectMany(t => t.Methods)
                                    .Where(m => m.HasCustomAttributes && !m.IsAbstract)
                                    .Select(m => new {
                                                    Method = m,
                                                    ProvAtts = m.CustomAttributes
                                                                    .Where(a => IsCuckooProviderType(a.AttributeType))
                                                    })
                                        .Where(t => t.ProvAtts.Any())
                                        .Select(t => { 
                                            int iCuckoo = 0;
                                            return new WeaveSpec() {
                                                            Method = t.Method,
                                                            ProvSpecs = t.ProvAtts
                                                                            .Select(a => new CuckooProvSpec() {
                                                                                Attribute = a,
                                                                                Index = iCuckoo++
                                                                            }).ToArray()
                                                            };
                                            });

            var weaves = weaveSpecs
                            .Select(spec => new MethodWeaver(spec, LogInfo));

            foreach(var weave in weaves.ToArray()) {
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



        bool IsCuckooProviderType(TypeReference typeRef) {
            var typeDef = typeRef.Resolve();

            return (typeDef.HasInterfaces 
                            && typeDef.Interfaces.Any(t => t.FullName == typeof(ICuckooProvider).FullName))
                    || (typeDef.BaseType != null 
                            && IsCuckooProviderType(typeDef.BaseType));
        }
        

    }
}
