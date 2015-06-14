using Cuckoo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class RefArgChangingCuckooAttribute : CuckooAttribute
    {
        public override void Usurp(ICall call) {
            call.CallInner();

            //DON'T KNOW ABOUT BELOW AS YET...
            
            var intRefArgs = call.Args.Where(a => a.Parameter.IsRetval
                                                  && a.Type == typeof(int));

            foreach(var intOutArg in intRefArgs) {
                intOutArg.Value = 999;
            }
        }
    }
}
