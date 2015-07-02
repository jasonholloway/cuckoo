using Cuckoo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    class ReturnChangingCuckoo2Attribute : CuckooAttribute
    {
        bool _returnsString;
        string _message;

        public ReturnChangingCuckoo2Attribute(string message) {
            _message = message;
        }

        public override void Init(IRoost roost) {
            _returnsString = ((MethodInfo)roost.Method).ReturnType == typeof(string);
        }

        public override void Call(ICall call) {
            call.CallInner();

            if(_returnsString) {
                call.ReturnValue = _message;
            }
        }
    }
}
