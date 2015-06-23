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
    public class AddingCuckooAttribute : CuckooAttribute
    {
        int _addendum;
        bool _returnsInt;


        public AddingCuckooAttribute(int addendum) {
            _addendum = addendum;
        }

        public override void OnRoost(IRoost roost) {
            throw new NotImplementedException();

            //_returnsInt = ((MethodInfo)roost.Method).ReturnType == typeof(int);
        }

        public override void OnCall(ICall call) {
            call.CallInner();

            if(_returnsInt) {
                call.ReturnValue = (int)call.ReturnValue + _addendum;
            }
        }
    }
}
