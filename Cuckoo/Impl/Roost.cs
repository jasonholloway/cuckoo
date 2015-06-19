using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Impl
{
    public class Roost : IRoost
    {
        public MethodBase Method { get; private set; }
        public ParameterInfo[] Parameters { get; private set; }
        public ICuckoo[] Cuckoos { get; private set; }

        public Roost(MethodBase method, ICuckoo[] cuckoos) {
            this.Method = method;
            this.Parameters = method.GetParameters();
            this.Cuckoos = cuckoos;
        }
    }
}
