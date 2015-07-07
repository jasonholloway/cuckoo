using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.EffusiveTargeterExample
{
    public class Class1
    {
        public void AMethod() {

        }
    }

    class Cuckoo : ICuckoo
    {
        public void Init(IRoost roost) {
            throw new NotImplementedException();
        }

        public void PreCall(ICallArg[] callArgs) {
            throw new NotImplementedException();
        }

        public void Call(ICall call) {
            throw new NotImplementedException();
        }
    }

    class Hatcher : ICuckooHatcher
    {
        public IEnumerable<ICuckoo> HatchCuckoos(IRoost roost) {
            yield return new Cuckoo();
        }
    }


    public class EffusiveTargeter : IRoostTargeter
    {
        public IEnumerable<RoostTarget> TargetRoosts(Assembly assembly) {
            return assembly.GetTypes()
                                .SelectMany(t => t.GetMethods())
                                .Select(m => new RoostTarget(m, typeof(Hatcher)));
        }
    }



}
