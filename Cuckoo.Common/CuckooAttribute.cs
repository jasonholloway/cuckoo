using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Common
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class CuckooAttribute : Attribute, ICallUsurper
    {
        public virtual void Init(MethodInfo method) { }
        public abstract void Usurp(ICall call);
    }
}
