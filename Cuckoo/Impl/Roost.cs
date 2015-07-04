using System.Linq;
using System.Reflection;

namespace Cuckoo.Impl
{
    public class Roost : IRoost
    {
        public MethodBase Method { get; private set; }
        public ParameterInfo[] Parameters { get; private set; }
        public ICuckoo[] Cuckoos { get; private set; }

        public Roost(MethodBase method) {
            Method = method;
            Parameters = method.GetParameters();
        }

        public void Init(ICuckooHatcher[] hatchers) {
            Cuckoos = hatchers.SelectMany(p => p.HatchCuckoos(this))
                                    .ToArray();

            foreach(var cuckoo in Cuckoos) {
                cuckoo.Init(this);
            }
        }        
    }
}
