using Mono.Cecil;
using Mono.Cecil.Rocks;
using Cuckoo.Weave.Cecil;
using Cuckoo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cuckoo.Gather;
using Cuckoo.Gather.Monikers;

namespace Cuckoo.Weave
{
    using NamedArg = KeyValuePair<string, object>;
    
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
                                                s => ((MethodDef)s.TargetMethod).MetadataToken,
                                                (k, r) => new {
                                                    MethodToken = k,
                                                    Hatchers = r.Select(f => new {
                                                                                Ctor = f.HatcherCtor,
                                                                                CtorArgs = f.HatcherCtorArgs,
                                                                                CtorNamedArgs = f.HatcherCtorNamedArgs
                                                                            })
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

                            var hatchWeaveSpecs = spec.Hatchers
                                                        .Select(h => {
                                                            var asm = _asmResolver
                                                                            .Resolve(AssemblyNameReference.Parse(h.Ctor.DeclaringType.AssemblyName));
                                                            
                                                            var mCtor = asm.MainModule
                                                                            .ImportMethodMoniker(h.Ctor, _asmResolver);

                                                            return new HatcherWeaveSpec(
                                                                                iProvWeaveSpec++, 
                                                                                mCtor,
                                                                                h.CtorArgs,
                                                                                h.CtorNamedArgs );
                                                        })
                                                        .ToArray();

                            return new RoostWeaveSpec(
                                                methodDef, 
                                                hatchWeaveSpecs );
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
