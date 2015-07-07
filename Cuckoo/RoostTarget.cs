using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo
{
    public struct RoostTarget
    {
        public readonly MethodBase TargetMethod;
        public readonly ConstructorInfo HatcherCtor;
        public readonly object[] HatcherCtorArgs;
        public readonly IDictionary<string, object> HatcherCtorNamedArgs;

        static object[] _emptyArgs = new object[0];
        static IDictionary<string, object> _emptyNamedArgs = new Dictionary<string, object>();


        public RoostTarget(MethodBase targetMethod, Type hatcherType) 
            : this(
                targetMethod, 
                hatcherType.GetConstructor(Type.EmptyTypes)
            ) { }

        public RoostTarget(
                MethodBase targetMethod, 
                ConstructorInfo hatcherCtor, 
                object[] hatcherCtorArgs = null,
                IDictionary<string, object> hatcherCtorNamedArgs = null) 
        {
            TargetMethod = targetMethod;
            HatcherCtor = hatcherCtor;
            HatcherCtorArgs = hatcherCtorArgs ?? _emptyArgs;
            HatcherCtorNamedArgs = hatcherCtorNamedArgs ?? _emptyNamedArgs;
        }
    }
}
