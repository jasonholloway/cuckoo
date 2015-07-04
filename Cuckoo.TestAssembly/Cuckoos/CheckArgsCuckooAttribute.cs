using Cuckoo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class ReturnCallArgTypesCuckooAttribute : CuckooAttribute
    {
        public override void Init(IRoost roost) {
            if(!(roost.Method is MethodInfo && ((MethodInfo)roost.Method).ReturnType == typeof(string[]))) {
                throw new InvalidOperationException("Cuckoo only fits methods returning string[]!");
            }
        }

        public override void Call(ICall call) {
            call.CallInner();

            call.ReturnValue = call.Args.Select(a => a.Type.FullName)
                                            .ToArray();
        }
    }


}
