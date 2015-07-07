using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Example
{
    class AnotherCuckoo : ICuckoo
    {
        public void Call(ICall call) {
            call.CallInner();

            if(call.ReturnValue.GetType() == typeof(string)) {
                call.ReturnValue = "Cuckooed!";
            }
        }

        public void Init(IRoost roost) {
            //...
        }

        public void PreCall(ICallArg[] callArgs) {
            //...
        }
    }
}
