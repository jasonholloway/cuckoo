using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    class WeaveContext
    {
        public ModuleDefinition Module;
        public TypeDefinition DeclaringType;
        public MethodDefinition InnerMethod;
        public MethodDefinition OuterMethod;
        public FieldDefinition CallSiteField;

        public NameSource NameSource;
        public RefMap RefMap;
        public Action<string> Logger;
    }
}
