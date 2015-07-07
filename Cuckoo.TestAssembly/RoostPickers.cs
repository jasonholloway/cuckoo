using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{

    public class RoostPickerClass : MarshalByRefObject 
    {
        public int RoostTarget(int i) {
            return i;
        }

    }


    public abstract class AbstractRoostTargeter : IRoostTargeter
    {
        public IEnumerable<RoostSpec> TargetRoosts(Assembly assembly) {
            throw new NotImplementedException();
        }
    }

    public class GenericRoostTargeter<T> : IRoostTargeter
    {
        public IEnumerable<RoostSpec> TargetRoosts(Assembly assembly) {
            throw new NotImplementedException();
        }
    }




    public class TestRoostPicker : IRoostTargeter
    {
        public IEnumerable<RoostSpec> TargetRoosts(Assembly assembly) 
        {
            var method = assembly
                            .GetType(typeof(RoostPickerClass).FullName)
                            .GetMethod("RoostTarget");

            yield return new RoostSpec(
                                    method,
                                    typeof(DeductingCuckooAttribute).GetConstructor(new[] { typeof(int) }),
                                    new object[] { 23 },
                                    new KeyValuePair<string, object>[0]);
        }
    }



}
