using Cuckoo.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class OptionalCtorArgsCuckooAttribute : CuckooAttribute
    {
        string _s;

        public OptionalCtorArgsCuckooAttribute(int i, string s = "blah") {
            _s = s;
        }

        public override void OnCall(ICall call) {

            call.CallInner();

            if(call.Method.ReturnType == typeof(string)) {
                call.ReturnValue = _s;
            }

        }
    }
}
