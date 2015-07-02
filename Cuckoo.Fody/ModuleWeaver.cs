using Mono.Cecil;
using Cuckoo.Fody.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cuckoo.Gather;

namespace Cuckoo.Fody
{
    public class ModuleWeaver
    {
        public string AssemblyFilePath { get; set; }
        public ModuleDefinition ModuleDefinition { get; set; }
        public Action<string> LogInfo { get; set; }




        TargetRoost[] GatherTargets() {
            var childAppDomain = AppDomain.CreateDomain("CuckooGathering");

            try {
                var targetAssembly = childAppDomain.Load(AssemblyName.GetAssemblyName(AssemblyFilePath));

                var agent = (GatherAgent)childAppDomain.CreateInstanceFromAndUnwrap(
                                                                typeof(GatherAgent).Assembly.Location,
                                                                typeof(GatherAgent).FullName);

                return agent.GatherAllRoostTargets(targetAssembly.FullName);
            }
            finally {
                AppDomain.Unload(childAppDomain);
            }

        }

        public void Execute() {

            var allTypes = ModuleDefinition.GetAllTypes().ToArray();

            var targets = GatherTargets();

            //group and resolve RoostTargets
            //to cecil defs
            //that is, convert em to WeaveSpecs
            
            var weaveSpecs = allTypes
                                .SelectMany(t => t.Methods)
                                    .Where(m => m.HasCustomAttributes && !m.IsAbstract)
                                    .Select(m => new {
                                                    Method = m,
                                                    ProvAtts = m.CustomAttributes
                                                                    .Where(a => a.AttributeType.ImplementsInterface<ICuckooProvider>())
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



    }
}
