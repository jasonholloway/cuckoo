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
        public CuckooSpec[] Cuckoos { get; set; }
    }

    internal class CuckooSpec
    {
        public CustomAttribute Attribute { get; set; }
        public int Index { get; set; }
    }

}
