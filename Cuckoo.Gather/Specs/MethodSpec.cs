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
        public readonly TypeSpec TypeSpec;
        public readonly string Name;
        public readonly int Token;

        public readonly MethodBase Method;

        public MethodSpec(TypeSpec typeSpec, int token, string name, MethodBase method) {
            TypeSpec = typeSpec;
            Token = token;
            Name = name;
            Method = method;
        }

        public MethodSpec(MethodBase method)
            : this(new TypeSpec(method.DeclaringType), method.MetadataToken, method.Name, method) { }

        public override string ToString() {
            return TypeSpec.Name + "." + Name;
        }
    }

}
