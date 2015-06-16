using Cuckoo.Common;
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
            var commonModule = ModuleDefinition.ReadModule("Cuckoo.Common.dll");

            var weaveSpecs = ModuleDefinition.Types
                                .SelectMany(t => t.Methods)
                                    .Where(m => m.HasCustomAttributes && !m.IsAbstract)
                                    .Select(m => new WeaveSpec() {
                                                            Method = m,
                                                            CuckooAttributes = m.CustomAttributes
                                                                                .Where(a => IsHatAttributeType(a.AttributeType))
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

        

        bool IsHatAttributeType(TypeReference typeRef) {
            if(typeRef.FullName == typeof(CuckooAttribute).FullName) {
                return true;
            }
            else {
                var baseType = typeRef.Resolve().BaseType;

                return baseType != null
                            ? IsHatAttributeType(baseType)
                            : false;
            }
        }

    }
}
