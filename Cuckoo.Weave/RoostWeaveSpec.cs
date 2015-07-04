using Mono.Cecil;
using System.Collections.Generic;

namespace Cuckoo.Weave
{
    using NamedArg = KeyValuePair<string, object>;

    internal struct RoostWeaveSpec
    {
        public readonly MethodDefinition Method;
        public readonly ProvWeaveSpec[] WeaveProvSpecs;

        public RoostWeaveSpec(MethodDefinition method, ProvWeaveSpec[] weaveProvSpecs) {
            Method = method;
            WeaveProvSpecs = weaveProvSpecs;
        }
    }

    internal struct ProvWeaveSpec
    {
        public readonly int Index;
        public readonly MethodReference CtorMethod;
        public readonly object[] CtorArgs;
        public readonly NamedArg[] NamedArgs;

        public ProvWeaveSpec(int index, MethodReference ctorMethod, object[] ctorArgs, NamedArg[] namedArgs) {
            Index = index;
            CtorMethod = ctorMethod;
            CtorArgs = ctorArgs;
            NamedArgs = namedArgs;
        }

    }

}
