using Mono.Cecil;
using Mono.Cecil.Rocks;
using Cuckoo.Fody.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cuckoo.Gather;

namespace Cuckoo.Fody
{
    using NamedArg = KeyValuePair<string, object>;

    public class ModuleWeaver
    {
        public string AssemblyFilePath { get; set; }
        public ModuleDefinition ModuleDefinition { get; set; }
        public Action<string> LogInfo { get; set; }




        IEnumerable<RoostSpec> GatherTargets() {
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

            var targets = GatherTargets();

            var groupedTargets = targets.GroupBy(
                                            t => t.Method.Token,
                                            (k, r) => new {
                                                MethodToken = k,
                                                CuckooProviderSpecs = r.Select(f => f.CuckooProvider)
                                                                          .ToArray()
                                            });
            
            var weaveRoostSpecs 
                = groupedTargets.Select(
                    target => {
                        var methodRef = (MethodReference)ModuleDefinition
                                                            .LookupToken(target.MethodToken);

                        var methodDef = methodRef as MethodDefinition;

                        if(methodDef == null) {
                            throw new InvalidOperationException(string.Format(
                                            "Can't add cuckoo to method {0}, as it isn't defined in current module {1}!", 
                                            methodRef.FullName, 
                                            ModuleDefinition.Name ));
                        }

                        int iProv = 0;

                        var provSpecs = target.CuckooProviderSpecs
                                                .Select(s => {
                                                    var ctorRef = (MethodReference)ModuleDefinition
                                                                                    .LookupToken(s.CtorToken);
                                                            
                                                    return new WeaveProvSpec(
                                                                        iProv++, 
                                                                        ctorRef,
                                                                        s.CtorArgs,
                                                                        s.NamedArgs );
                                                })
                                                .ToArray();


                        return new WeaveRoostSpec(
                                            methodDef, 
                                            provSpecs );
                    });


            var weaves = weaveRoostSpecs
                            .Where(spec => !spec.Method.IsAbstract)
                            .Select(spec => new MethodWeaver(spec, LogInfo))
                            .ToArray();

            foreach(var weave in weaves) {
                weave.Weave();
            }
        }



    }
}
