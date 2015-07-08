using Cuckoo.Gather.Monikers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo.Gather.Specs
{

    [Serializable]
    public class MethodSpec
    {
        public readonly ITypeMoniker Type;
        public readonly string Name;
        public readonly int Token;

        public readonly MethodBase Method;

        public MethodSpec(ITypeMoniker typeMoniker, int token, string name, MethodBase method) {
            Type = typeMoniker;
            Token = token;
            Name = name;
            Method = method;
        }

        public MethodSpec(MethodBase method)
            : this(TypeMoniker.Derive(method.DeclaringType), method.MetadataToken, method.Name, method) { }

        public override string ToString() {
            return Name;
        }
    }

}
