using Cuckoo.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.AnotherTestAssembly
{
    public class DistantCuckooAttribute : CuckooAttribute
    {
        public override void Call(ICall call) {            
            call.CallInner();
            call.ReturnValue = 999;
        }
    }
}
