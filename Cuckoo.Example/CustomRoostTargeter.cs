using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Example
{
    class CustomRoostTargeter : IRoostTargeter
    {
        public IEnumerable<RoostTarget> TargetRoosts(Assembly asm) {
            var methods = asm.GetType(typeof(AClass).FullName)
                                .GetMethods()
                                .Where(m => m.DeclaringType == typeof(AClass));

            return methods
                    .Select(m => new RoostTarget(m, typeof(SimpleHatcher<AnotherCuckoo>)));        
        }
    }
}
