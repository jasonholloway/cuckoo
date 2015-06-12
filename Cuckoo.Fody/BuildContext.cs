using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    class BuildContext
    {
        public ModuleDefinition Module;
        public TypeDefinition DeclaringType;
        public MethodDefinition InnerMethod;
        public MethodDefinition OuterMethod;
        public ElementNameSource NameSource;
        public Ref Ref;
    }
}
