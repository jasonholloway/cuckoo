using System;

namespace Cuckoo.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public abstract class CuckooAttribute : Attribute, ICuckoo
    {
        public virtual void OnRoost(IRoost roost) { }
        public abstract void OnCall(ICall call);
    }
}
