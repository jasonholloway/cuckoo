using Mono.Cecil;
using System.Collections.Generic;

namespace Cuckoo.Fody
{
    using NamedArg = KeyValuePair<string, object>;

    internal struct WeaveRoostSpec
    {
        public readonly MethodDefinition Method;
        public readonly WeaveProvSpec[] WeaveProvSpecs;

        public WeaveRoostSpec(MethodDefinition method, WeaveProvSpec[] weaveProvSpecs) {
            Method = method;
            WeaveProvSpecs = weaveProvSpecs;
        }
    }

    internal struct WeaveProvSpec
    {
        public readonly int Index;
        public readonly MethodReference CtorMethod;
        public readonly object[] CtorArgs;
        public readonly NamedArg[] NamedArgs;

        public WeaveProvSpec(int index, MethodReference ctorMethod, object[] ctorArgs, NamedArg[] namedArgs) {
            Index = index;
            CtorMethod = ctorMethod;
            CtorArgs = ctorArgs;
            NamedArgs = namedArgs;
        }




        //and arg info here please

        public CustomAttribute Attribute { get { return null; } }
    }

}
