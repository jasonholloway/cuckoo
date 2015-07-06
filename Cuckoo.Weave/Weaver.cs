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
    using Cuckoo.Common;

    public class Weaver
    {
        AssemblyDefinition _asmDef;
        IEnumerable<RoostSpec> _roostSpecs;
        Logger _log;
        IAssemblyResolver _asmResolver;


        public Weaver(
                AssemblyDefinition assemblyDef,
                IEnumerable<RoostSpec> roostSpecs, 
                Logger log ) 
        {
            _asmDef = assemblyDef;
            _roostSpecs = roostSpecs;
            _log = log;

            _asmResolver = new CachedAssemblyResolver(assemblyDef.MainModule.AssemblyResolver);
            ((CachedAssemblyResolver)_asmResolver).Register(assemblyDef);
        }


        public void Weave() {
            foreach(var module in _asmDef.Modules) {
                WeaveModule(module);
            }
        }


        public void WeaveModule(ModuleDefinition module) 
        {
            var groupedRoostSpecs = _roostSpecs.GroupBy(
                                                s => s.MethodSpec.Token,
                                                (k, r) => new {
                                                    MethodToken = k,
                                                    HatcherSpecs = r.Select(f => f.HatcherSpec)
                                                                         .ToArray()
                                                });
            
            var roostWeaveSpecs 
                = groupedRoostSpecs.Select(
                        spec => {
                            var methodRef = (MethodReference)module.LookupToken(spec.MethodToken);

                            var methodDef = methodRef as MethodDefinition;

                            if(methodDef == null) {
                                throw new InvalidOperationException(string.Format(
                                                "Can't add cuckoo to method {0}, as it isn't defined in current module {1}!", 
                                                methodRef.FullName, 
                                                module.Name ));
                            }

                            int iProvWeaveSpec = 0;

                            var provWeaveSpecs = spec.HatcherSpecs
                                                        .Select(s => {
                                                            var asm = _asmResolver
                                                                        .Resolve(AssemblyNameReference.Parse(s.AssemblyName));

                                                            var ctorRef = (MethodReference)asm.MainModule
                                                                                                .LookupToken(s.CtorToken);
                                                            
                                                            return new HatcherWeaveSpec(
                                                                                iProvWeaveSpec++, 
                                                                                ctorRef,
                                                                                s.CtorArgs,
                                                                                s.NamedArgs );
                                                        })
                                                        .ToArray();

                            return new RoostWeaveSpec(
                                                methodDef, 
                                                provWeaveSpecs );
                        });
            

            var roostWeavers = roostWeaveSpecs
                                    .Where(s => !s.Method.IsAbstract)
                                    .Select(s => new RoostWeaver(s, _log));
            
            foreach(var roostWeaver in roostWeavers.ToArray()) {
                roostWeaver.Weave();
            }
        }

    }
}
