using Mono.Cecil;
using System.Collections.Generic;

namespace Cuckoo.Weave
{
    using NamedArg = KeyValuePair<string, object>;

    internal struct RoostWeaveSpec
    {
        public readonly MethodDefinition Method;
        public readonly HatcherWeaveSpec[] HatcherSpecs;

        public RoostWeaveSpec(MethodDefinition method, HatcherWeaveSpec[] hatcherSpecs) {
            Method = method;
            HatcherSpecs = hatcherSpecs;
        }
    }

    internal struct HatcherWeaveSpec
    {
        public readonly int Index;
        public readonly MethodReference CtorMethod;
        public readonly object[] CtorArgs;
        public readonly NamedArg[] NamedArgs;

        public HatcherWeaveSpec(int index, MethodReference ctorMethod, object[] ctorArgs, NamedArg[] namedArgs) {
            Index = index;
            CtorMethod = ctorMethod;
            CtorArgs = ctorArgs;
            NamedArgs = namedArgs;
        }

    }

}
