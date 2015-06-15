using Cuckoo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class BareCuckooAttribute : CuckooAttribute
    {
        public override void Init(MethodInfo method) {
            //...
        }
        public override void Usurp(ICall call) {

            var fReturn = call.GetType().GetField("_return");

            object vBefore = fReturn.GetValue(call);

            call.CallInner();

            object vAfter = fReturn.GetValue(call);

        }
    }


}
