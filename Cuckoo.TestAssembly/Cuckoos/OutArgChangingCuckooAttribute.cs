using Cuckoo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class OutArgChangingCuckooAttribute : CuckooAttribute
    {
        public override void Usurp(ICall call) {
            call.CallInner();

            var intOutArgs = call.Args.Where(a => a.Parameter.IsOut 
                                                  && a.Type == typeof(int));
            
            foreach(var intOutArg in intOutArgs) {
                intOutArg.Value = 999;
            }
        }
    }
}
