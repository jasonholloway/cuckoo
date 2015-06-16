using Cuckoo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class ChangeByRefIntCuckooAttribute : CuckooAttribute
    {
        int _i;

        public ChangeByRefIntCuckooAttribute(int i) {
            _i = i;
        }

        public override void Usurp(ICall call) {
            call.CallInner();

            //change byref int args
            //...
        }
    }
}
