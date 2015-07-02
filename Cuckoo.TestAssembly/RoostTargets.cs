using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class SimpleRoostTargeter : IRoostPicker
    {
        public static int InstanceCount = 0;
        public static int RunCount = 0;

        public SimpleRoostTargeter() {
            InstanceCount++;
        }

        public IEnumerable<RoostSpec> PickRoosts(Assembly assembly) {
            RunCount++;
            return new RoostSpec[0];
            
            //{
            //    new RoostSpec(
            //            new MethodSpec("Method1", "Type1", 1), 
            //            new CuckooProviderSpec() ),
            //    new RoostSpec(
            //            new MethodSpec("Method2", "Type2", 2), 
            //            new CuckooProviderSpec() ),
            //};
        }
    }



}
