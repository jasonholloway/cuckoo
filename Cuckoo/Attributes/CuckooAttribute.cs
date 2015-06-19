using System;

namespace Cuckoo.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public abstract class CuckooAttribute : Attribute, ICuckoo
    {
        public virtual void OnRoost(IRoost roost) { }

        public virtual void OnBeforeCall(ICall call) {
            //call.Proceed();
        }

        public virtual void OnCall(ICall call) {
            call.CallInner();
        }
    }
}
