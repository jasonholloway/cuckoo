using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Gather.Specs
{
    using Cuckoo.Gather.Monikers;
    using System.Reflection;
    using NamedArg = KeyValuePair<string, object>;
    
    [Serializable]
    public class HatcherSpec
    {
        public readonly ITypeMoniker Type;
        public readonly int CtorToken;
        public readonly object[] CtorArgs;
        public readonly NamedArg[] NamedArgs;

        public HatcherSpec(
                ITypeMoniker typeMoniker,
                int ctorToken,
                object[] ctorArgs,
                NamedArg[] namedArgs) 
        {
            Type = typeMoniker;
            CtorToken = ctorToken;
            CtorArgs = ctorArgs;
            NamedArgs = namedArgs;
        }

        public HatcherSpec(ConstructorInfo ctor, object[] args, NamedArg[] namedArgs)
            : this(
                TypeMoniker.Derive(ctor.DeclaringType),
                ctor.MetadataToken,
                args,
                namedArgs) { }

        public override string ToString() {
            return Type.GetAssemblyQualifiedName();
        }
    }

}
