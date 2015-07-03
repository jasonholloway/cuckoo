﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo
{
    using NamedArg = KeyValuePair<string, object>;


    [Serializable]
    public struct RoostSpec
    {
        public readonly MethodSpec Method;
        public readonly CuckooProviderSpec CuckooProvider;

        public RoostSpec(MethodSpec method, CuckooProviderSpec cuckooProvider) {
            Method = method;
            CuckooProvider = cuckooProvider;
        }

        public RoostSpec(MethodBase method, ConstructorInfo ctor, object[] args, NamedArg[] namedArgs)
            : this(new MethodSpec(method), new CuckooProviderSpec(ctor, args, namedArgs)) { }

        public RoostSpec(MethodBase method, Type cuckooProviderType)
            : this(method, cuckooProviderType.GetConstructor(Type.EmptyTypes), new object[0], new NamedArg[0]) { }

        public override string ToString() {
            return string.Format("{0} <- {1}", Method, CuckooProvider);
        }
    }

    [Serializable]
    public struct MethodSpec
    {
        public readonly string Name;
        public readonly string TypeName;
        public readonly int Token;

        public MethodSpec(string name, string typeName, int token) {
            Name = name;
            TypeName = typeName;
            Token = token;
        }

        public MethodSpec(MethodBase method)
            : this(method.Name, method.DeclaringType.FullName, method.MetadataToken) { }

        public override string ToString() {
            return TypeName + "." + Name;
        }
    }


    [Serializable]
    public struct CuckooProviderSpec
    {
        public readonly string Name;
        public readonly string AssemblyName;
        public readonly int CtorToken;
        public readonly object[] CtorArgs;
        public readonly NamedArg[] NamedArgs;

        public CuckooProviderSpec(string name, string assemblyName, int ctorToken, object[] ctorArgs, NamedArg[] namedArgs) {
            Name = name;
            AssemblyName = assemblyName;
            CtorToken = ctorToken;
            CtorArgs = ctorArgs;
            NamedArgs = namedArgs;
        }

        public CuckooProviderSpec(ConstructorInfo ctor, object[] args, NamedArg[] namedArgs)
            : this(ctor.DeclaringType.FullName, ctor.DeclaringType.Assembly.FullName, ctor.MetadataToken, args, namedArgs) { }

        public override string ToString() {
            return Name;
        }
    }

}
