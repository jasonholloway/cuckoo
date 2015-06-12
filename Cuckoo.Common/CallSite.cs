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
        public ParameterInfo[] Parameters { get; private set; }
        public ICallUsurper[] Usurpers { get; private set; }

        public CallSite(MethodInfo method, ICallUsurper[] usurpers) {
            this.Method = method;
            this.Parameters = method.GetParameters();
            this.Usurpers = usurpers;
        }
    }
}
