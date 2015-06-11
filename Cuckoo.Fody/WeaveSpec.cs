using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    internal class WeaveSpec
    {
        public MethodDefinition Method { get; set; }
        public CustomAttribute[] CuckooAttributes { get; set; }
    }
}
