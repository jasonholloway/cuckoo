using System;

namespace Cuckoo.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public abstract class CuckooAttribute : Attribute, ICuckoo
    {
        public virtual void Init(IRoost roost) { }

        public virtual void PreCall(IBeforeCall call) {
            //call.Proceed();
        }

        public virtual void Call(ICall call) {
            call.CallInner();
        }
    }
}
