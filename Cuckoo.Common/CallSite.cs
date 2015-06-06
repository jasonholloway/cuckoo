using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Common
{
    public class CallSite
    {
        public MethodInfo Method { get; private set; }
        public ICallUsurper Usurper { get; private set; }

        public CallSite(MethodInfo method, ICallUsurper usurper) {
            this.Method = method;
            this.Usurper = usurper;
        }
    }
}
