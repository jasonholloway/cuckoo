using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Cuckoo.Gather.Monikers;

namespace Cuckoo.Gather
{
    using NamedArg = KeyValuePair<string, object>;
    
    [Serializable]
    public class RoostSpec
    {
        public readonly IMethodMoniker TargetMethod;

        public readonly IMethodMoniker HatcherCtor;
        public readonly object[] HatcherCtorArgs;
        public readonly NamedArg[] HatcherCtorNamedArgs;

        public RoostSpec(
                    IMethodMoniker targetMethod, 
                    IMethodMoniker hatcherCtor,
                    object[] hatcherCtorArgs = null,
                    NamedArg[] hatcherCtorNamedArgs = null
                    ) 
        {
            TargetMethod = targetMethod;
            HatcherCtor = hatcherCtor;
            HatcherCtorArgs = hatcherCtorArgs ?? new object[0];
            HatcherCtorNamedArgs = hatcherCtorNamedArgs ?? new NamedArg[0];
        }

        public override string ToString() {
            return string.Format(
                            "{0} <- {1}", 
                            TargetMethod.FullName, 
                            HatcherCtor.DeclaringType.Name);
        }
    }


}
