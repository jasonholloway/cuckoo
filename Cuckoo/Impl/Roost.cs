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
        public Method Method { get; private set; }
        public ICuckoo[] Cuckoos { get; private set; }

        public Roost(Method method, ICuckoo[] cuckoos) {
            Method = method;
            Cuckoos = cuckoos;
        }
    }
}
