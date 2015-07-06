using Cuckoo.Common;
using Mono.Cecil;
using System;

namespace Cuckoo.Weave
{
    class WeaveContext
    {
        public ModuleDefinition Module;
        public TypeDefinition tCont;
        public TypeReference tContRef;
        public MethodDefinition mInner;
        public MethodDefinition mOuter;

        public NameSource NameSource;
        public CommonRefs RefMap;
        public Logger Logger;
    }
}
