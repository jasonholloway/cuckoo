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
    public class ReturnCallArgsCuckooAttribute : CuckooAttribute
    {
        public override void Init(IRoost roost) {
            if(!(roost.Method is MethodInfo && ((MethodInfo)roost.Method).ReturnType == typeof(ICallArg[]))) {
                throw new InvalidOperationException("Cuckoo only fits methods returning ICallArg[]!");
            }
        }

        public override void Call(ICall call) {
            call.CallInner();

            call.ReturnValue = call.Args;
        }
    }


}
