using System;
using System.Collections.Generic;

namespace Cuckoo
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public abstract class CuckooAttribute : Attribute, ICuckooHatcher, ICuckoo
    {
        IEnumerable<ICuckoo> ICuckooHatcher.HatchCuckoos(IRoost roost) {
            return new ICuckoo[] { this };
        }

        public virtual void Init(IRoost roost) { }

        public virtual void PreCall(ICallArg[] callArgs) { }

        public virtual void Call(ICall call) {
            call.CallInner();
        }
    }
}
