using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Gather.Specs
{
    [Serializable]
    public class TypeSpec
    {
        public readonly string AssemblyName;
        public readonly string FullName;
        public readonly TypeSpec[] GenArgs;
        public readonly bool IsArray;

        public readonly Type Type;

        public TypeSpec(string asmName, string fullName, TypeSpec[] genArgs, bool isArray, Type type) {
            AssemblyName = asmName;
            FullName = fullName;
            GenArgs = genArgs;
            IsArray = isArray;
            Type = type;
        }

        public TypeSpec(Type type) {
            AssemblyName = type.Assembly.FullName;
            FullName = type.FullName;
            GenArgs = type.IsGenericType
                        ? type.GetGenericArguments()
                                .Select(a => new TypeSpec(a))
                                .ToArray()
                        : new TypeSpec[0];
            IsArray = type.IsArray;
            Type = type;
        }
    }
}
