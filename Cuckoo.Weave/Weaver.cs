using Mono.Cecil;
using Mono.Cecil.Rocks;
using Cuckoo.Weave.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cuckoo.Gather;

namespace Cuckoo.Weave
{
    using NamedArg = KeyValuePair<string, object>;

    public class Weaver
    {
        string _asmPath;
        ModuleDefinition _module;
        Action<string> _fnLog;

        public void Init(ModuleDefinition module, string assemblyFilePath, Action<string> fnLog) {
            _module = module;
            _asmPath = assemblyFilePath;
            _fnLog = fnLog;
        }
        

        IEnumerable<RoostSpec> GatherTargets() 
        {
            var childAppDomain = AppDomain.CreateDomain(
                                            "CuckooGathering",
                                            null,
                                            new AppDomainSetup() {
                                                ShadowCopyFiles = "true"
                                            });
            try {
                var targetAssembly = childAppDomain.Load(AssemblyName.GetAssemblyName(_asmPath));

                var agent = (GatherAgent)childAppDomain.CreateInstanceFromAndUnwrap(
                                                                typeof(GatherAgent).Assembly.Location,
                                                                typeof(GatherAgent).FullName);

                return agent.GatherAllRoostTargets(targetAssembly.FullName);
            }
            finally {
                AppDomain.Unload(childAppDomain);
            }

        }


        public void Weave() 
        {
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
                        var methodRef = (MethodReference)_module.LookupToken(target.MethodToken);

                        var methodDef = methodRef as MethodDefinition;

                        if(methodDef == null) {
                            throw new InvalidOperationException(string.Format(
                                            "Can't add cuckoo to method {0}, as it isn't defined in current module {1}!", 
                                            methodRef.FullName, 
                                            _module.Name ));
                        }

                        int iProv = 0;

                        var provSpecs = target.CuckooProviderSpecs
                                                .Select(s => {
                                                    var asm = _module.AssemblyResolver
                                                                                .Resolve(AssemblyNameReference.Parse(s.AssemblyName));

                                                    var ctorRef = (MethodReference)asm.MainModule
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
                            .Select(spec => new MethodWeaver(spec, _fnLog))
                            .ToArray();


            foreach(var weave in weaves) {
                weave.Weave();
            }
        }



    }
}
