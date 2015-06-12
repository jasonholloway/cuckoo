using Cuckoo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class ArgChangingCuckooAttribute : CuckooAttribute
    {
        public override void Usurp(ICall call) {
            var stringArgs = call.Args
                                .Where(a => a.Parameter.ParameterType == typeof(string));

            foreach(var stringArg in stringArgs) {
                stringArg.Value = "Growl";
            }

            call.CallInner();
        }
    }
}
