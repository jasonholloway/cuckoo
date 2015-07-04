using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{

    public class RoostTargetClass : MarshalByRefObject 
    {
        public int RoostTarget(int i) {
            return i;
        }
    }



    public class TestRoostTargeter : IRoostPicker
    {
        public IEnumerable<RoostSpec> PickRoosts(Assembly assembly) {

            var method = assembly
                            .GetType(typeof(RoostTargetClass).FullName)
                            .GetMethod("RoostTarget");

            yield return new RoostSpec(
                                    method,
                                    typeof(DeductingCuckooAttribute).GetConstructor(new[] { typeof(int) }),
                                    new object[] { 23 },
                                    new KeyValuePair<string, object>[0]);
        }
    }



}
