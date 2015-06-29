using System;
using System.Collections.Generic;

namespace Cuckoo.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public abstract class CuckooAttribute : Attribute, ICuckoo, ICuckooProvider
    {
        public virtual void Init(IRoost roost) { }

        public virtual void PreCall(ICallArg[] callArgs) { }

        public virtual void Call(ICall call) {
            call.CallInner();
        }

        IEnumerable<ICuckoo> ICuckooProvider.CreateCuckoos(IRoost roost) {
            return new ICuckoo[] { this };
        }
    }
}
