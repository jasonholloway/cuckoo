using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo.Gather.Specs
{
    using Cuckoo.Gather.Monikers;
    using NamedArg = KeyValuePair<string, object>;
    
    [Serializable]
    public class RoostSpec
    {
        public readonly IMethodMoniker Method;
        public readonly HatcherSpec HatcherSpec;

        public RoostSpec(IMethodMoniker method, HatcherSpec hatcherSpec) {
            Method = method;
            HatcherSpec = hatcherSpec;
        }

        public RoostSpec(MethodBase method, ConstructorInfo ctor, object[] args, NamedArg[] namedArgs)
            : this(MethodMoniker.Derive(method), new HatcherSpec(ctor, args, namedArgs)) { }

        public RoostSpec(MethodBase method, Type cuckooHatcherType)
            : this(method, cuckooHatcherType.GetConstructor(Type.EmptyTypes), new object[0], new NamedArg[0]) { }

        public override string ToString() {
            return string.Format("{0} <- {1}", Method.FullName, HatcherSpec);
        }
    }


}
