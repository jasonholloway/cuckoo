using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class CuckooAttribute : Attribute, ICuckoo
    {
        public virtual void OnRoost(IRoost roost) { }
        public abstract void OnCall(ICall call);
    }
}
