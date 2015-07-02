using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo
{
    public interface IRoostTargeter
    {
        IEnumerable<TargetRoost> GetTargets(Assembly assembly);    
    }
    

    [Serializable]
    public struct TargetRoost
    {
        public readonly TargetMethod Method;
        public readonly TargetCuckooProvider CuckooProvider;

        public TargetRoost(TargetMethod method, TargetCuckooProvider cuckooProvider) {
            Method = method;
            CuckooProvider = cuckooProvider;
        }

        public TargetRoost(MethodBase method, Type cuckooProviderType) 
            : this(new TargetMethod(method), new TargetCuckooProvider(cuckooProviderType)) { }


        public override string ToString() {
            return string.Format("{0} <- {1}", Method, CuckooProvider);
        }
    }

    [Serializable]
    public struct TargetMethod
    {
        public readonly string Name;
        public readonly string TypeName;
        public readonly int Token;

        public TargetMethod(string name, string typeName, int token) {
            Name = name;
            TypeName = typeName;
            Token = token;
        }

        public TargetMethod(MethodBase method) 
            : this(method.Name, method.DeclaringType.FullName, method.MetadataToken) {}
        
        public override string ToString() {
            return TypeName + "." + Name;
        }
    }

    [Serializable]
    public struct TargetCuckooProvider
    {
        public readonly string Name;
        public readonly int Token;
        public readonly TargetArgument[] CtorArgs;

        public TargetCuckooProvider(string name, int token, TargetArgument[] ctorArgs) {
            Name = name;
            Token = token;
            CtorArgs = ctorArgs;
        }

        public TargetCuckooProvider(Type type) 
            : this(type.FullName, type.MetadataToken, new TargetArgument[0]) { }

        public override string ToString() {
            return Name;
        }
    }

    [Serializable]
    public struct TargetArgument
    {
        public readonly string Name;
        public readonly byte[] ValueBlob;

        public TargetArgument(string name, byte[] valueBlob) {
            Name = name;
            ValueBlob = valueBlob;
        }

        public override string ToString() {
            return Name;
        }
    }

}
