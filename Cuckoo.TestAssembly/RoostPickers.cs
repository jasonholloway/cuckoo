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


    public abstract class AbstractTargeter : IRoostTargeter
    {
        public IEnumerable<RoostTarget> TargetRoosts(Assembly assembly) {
            throw new NotImplementedException();
        }
    }

    public class GenericTargeter<T> : IRoostTargeter
    {
        public IEnumerable<RoostTarget> TargetRoosts(Assembly assembly) {
            throw new NotImplementedException();
        }
    }




    public class TestTargeter : IRoostTargeter
    {
        public IEnumerable<RoostTarget> TargetRoosts(Assembly assembly) 
        {
            var method = assembly
                            .GetType(typeof(RoostPickerClass).FullName)
                            .GetMethod("RoostTarget");

            yield return new RoostTarget(
                                    method,
                                    typeof(DeductingCuckooAttribute).GetConstructor(new[] { typeof(int) }),
                                    new object[] { 23 }
                                    );
        }
    }



}
