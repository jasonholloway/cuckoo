using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Example
{
    internal class TestCuckooAttribute : CuckooAttribute
    {
        public override void Call(ICall call) {
            base.Call(call);

            if(call.ReturnValue.GetType() == typeof(string)) {
                call.ReturnValue = "Cuckooed by attribute!";
            }
        }
    }
}
