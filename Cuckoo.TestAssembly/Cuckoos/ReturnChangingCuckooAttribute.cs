using Cuckoo.Common;
using Cuckoo.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    class ReturnChangingCuckooAttribute : CuckooAttribute
    {
        bool _returnsString;
        string _message;

        public ReturnChangingCuckooAttribute(string message) {
            _message = message;
        }

        public override void OnRoost(IRoost roost) {
            _returnsString = roost.Method.ReturnType == typeof(string);
        }

        public override void OnCall(ICall call) {
            call.CallInner();

            if(_returnsString) {
                call.ReturnValue = _message;
            }
        }
    }
}
