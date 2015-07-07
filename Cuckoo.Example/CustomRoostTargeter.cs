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
        public IEnumerable<RoostSpec> TargetRoosts(Assembly asm) {
            var methods = asm.GetType(typeof(AClass).FullName)
                                .GetMethods();

            return methods
                    .Select(m => new RoostSpec(m, typeof(SimpleHatcher<AnotherCuckoo>)));        
        }
    }
}
