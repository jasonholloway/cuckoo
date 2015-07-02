using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class SimpleRoostTargeter : IRoostTargeter
    {
        public static int InstanceCount = 0;
        public static int RunCount = 0;

        public SimpleRoostTargeter() {
            InstanceCount++;
        }

        public IEnumerable<TargetRoost> GetTargets(Assembly assembly) {
            RunCount++;
            return new TargetRoost[] {
                new TargetRoost(
                        new TargetMethod("Method1", "Type1", 1), 
                        new TargetCuckooProvider() ),
                new TargetRoost(
                        new TargetMethod("Method2", "Type2", 2), 
                        new TargetCuckooProvider() ),
            };
        }
    }



}
