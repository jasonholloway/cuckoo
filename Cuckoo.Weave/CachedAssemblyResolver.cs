using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Weave
{
    public class CachedAssemblyResolver : IAssemblyResolver
    {
        IAssemblyResolver _inner;
        Dictionary<string, AssemblyDefinition> _cache;

        public CachedAssemblyResolver(IAssemblyResolver innerResolver) {
            _inner = innerResolver;
            _cache = new Dictionary<string, AssemblyDefinition>();
        }

        public void Register(AssemblyDefinition asmDef) {
            _cache[asmDef.FullName] = asmDef;
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters) {
            AssemblyDefinition asmDef;

            if(!_cache.TryGetValue(fullName, out asmDef)) {
                asmDef = _inner.Resolve(fullName, parameters);
                _cache[fullName] = asmDef;
            }

            return asmDef;
        }

        public AssemblyDefinition Resolve(string fullName) {
            AssemblyDefinition asmDef;

            if(!_cache.TryGetValue(fullName, out asmDef)) {
                asmDef = _inner.Resolve(fullName);
                _cache[fullName] = asmDef;
            }

            return asmDef;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters) {
            AssemblyDefinition asmDef;

            string fullName = name.FullName;

            if(!_cache.TryGetValue(fullName, out asmDef)) {
                asmDef = _inner.Resolve(name, parameters);
                _cache[fullName] = asmDef;
            }

            return asmDef;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name) {
            AssemblyDefinition asmDef;

            string fullName = name.FullName;

            if(!_cache.TryGetValue(fullName, out asmDef)) {
                asmDef = _inner.Resolve(name);
                _cache[fullName] = asmDef;
            }

            return asmDef;
        }
    }

}
