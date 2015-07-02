using Cuckoo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class CheckInstanceInPlaceCuckooAttribute : CuckooAttribute
    {
        public override void Call(ICall call) {
            if(call.Instance == null) {
                throw new InvalidOperationException("No instance in place!!!");
            }

            call.CallInner();
        }
    }


}
