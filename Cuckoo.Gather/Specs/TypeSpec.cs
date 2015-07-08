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
        public readonly string Namespace;
        public readonly string Name;
        public readonly TypeSpec ParentType;
        public readonly TypeSpec[] GenTypes;
        public readonly bool IsArray;

        public TypeSpec(
                string asmName, 
                string @namespace, 
                string name, 
                TypeSpec parentType, 
                TypeSpec[] genTypes, 
                bool isArray) 
        {
            AssemblyName = asmName;
            Namespace = @namespace;
            Name = name;
            ParentType = parentType;
            GenTypes = genTypes;
            IsArray = isArray;
        }


        public TypeSpec(Type type)
        {
            AssemblyName = type.Assembly.FullName;

            Namespace = type.Namespace;

            Name = type.IsArray || (type.IsGenericType && !type.IsGenericTypeDefinition) 
                    ? type.GetElementType().Name
                    : type.Name;

            ParentType = type.IsNested
                            ? new TypeSpec(type.DeclaringType)
                            : null;

            GenTypes = type.IsGenericType && !type.IsGenericTypeDefinition
                            ? type.GetGenericArguments()
                                    .Select(a => new TypeSpec(a))
                                    .ToArray()
                            : new TypeSpec[0];

            IsArray = type.IsArray;
        }
    }
}
