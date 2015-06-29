using Mono.Cecil;

namespace Cuckoo.Fody
{
    internal class WeaveSpec
    {
        public MethodDefinition Method { get; set; }
        public CuckooProvSpec[] ProvSpecs { get; set; }
    }

    internal class CuckooProvSpec
    {
        public CustomAttribute Attribute { get; set; }
        public int Index { get; set; }
    }

}
