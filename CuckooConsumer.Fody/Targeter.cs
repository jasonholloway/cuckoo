using Cuckoo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CuckooConsumer
{
    //On compilation
    class Targeter : IRoostTargeter
    {
        public IEnumerable<RoostTarget> TargetRoosts(Assembly assembly) 
        {
            var methods = assembly.ManifestModule.GetMethods().Where(m => m.Name.StartsWith("J"));

            foreach(var method in methods) {
                yield return new RoostTarget(method, typeof(Hatcher));
            }
        }
    }
    

    //On target module load
    //Allows different cuckoos to be emplaced at run-time based on the particular roost targeted
    //(possibility that different targeters may have converged on same method...)
    class Hatcher : ICuckooHatcher
    {
        public IEnumerable<ICuckoo> HatchCuckoos(IRoost roost) {
            yield return new ClockingCuckoo();
        }
    }



    //'Hatched' into its 'roost' by the above; stays in place for duration of program
    class ClockingCuckoo : ICuckoo
    {
        public void Init(IRoost roost) {
            //Called only once, on very first call of cuckooed method
        }

        public void PreCall(ICallArg[] callArgs) {
            //Niche usage: before cuckooed ctor invocation
            //Allows access to args before allocation of instance object 
            //SHOULD REALLY LET US USURP ALLOCATION! still to do...
        }

        public void Call(ICall call) {
            //The main hook: can delegate inwards to original method, but don't have to.
            CuckooCount.CallTimes.Enqueue(DateTime.Now);
            call.CallInner();                        
        }
    }

}
