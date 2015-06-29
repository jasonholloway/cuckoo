using Mono.Cecil;
using System;

namespace Cuckoo.Fody
{
    class WeaveContext
    {
        public ModuleDefinition Module;
        public TypeDefinition tCont;
        public TypeReference tContRef;
        public MethodDefinition mInner;
        public MethodDefinition mOuter;
        public FieldDefinition fRoost;

        public NameSource NameSource;
        public RefMap RefMap;
        public Action<string> Logger;
    }
}
