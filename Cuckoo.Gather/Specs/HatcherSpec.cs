using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Gather.Specs
{
    using System.Reflection;
    using NamedArg = KeyValuePair<string, object>;
    
    [Serializable]
    public class HatcherSpec
    {
        public readonly TypeSpec TypeSpec;
        public readonly int CtorToken;
        public readonly object[] CtorArgs;
        public readonly NamedArg[] NamedArgs;

        public HatcherSpec(
                TypeSpec typeSpec,
                int ctorToken,
                object[] ctorArgs,
                NamedArg[] namedArgs) {
            TypeSpec = typeSpec;
            CtorToken = ctorToken;
            CtorArgs = ctorArgs;
            NamedArgs = namedArgs;
        }

        public HatcherSpec(ConstructorInfo ctor, object[] args, NamedArg[] namedArgs)
            : this(
                new TypeSpec(ctor.DeclaringType),
                ctor.MetadataToken,
                args,
                namedArgs) { }

        public override string ToString() {
            return TypeSpec.Name;
        }
    }

}
