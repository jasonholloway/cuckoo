using Cuckoo;
using Cuckoo.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class DeductingCuckooAttribute : CuckooAttribute
    {
        int _subtrahend;
        bool _returnsInt;

        public DeductingCuckooAttribute(int subtrahend) {
            _subtrahend = subtrahend;
        }

        public override void OnRoost(IRoost roost) {
            _returnsInt = ((MethodInfo)roost.Method).ReturnType == typeof(int);
        }

        public override void OnInvoke(ICall call) {
            call.CallInner();

            if(_returnsInt) {
                call.ReturnValue = (int)call.ReturnValue - _subtrahend;
            }
        }
    }
}
