using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class OptionalAttArgsCuckooAttribute : CuckooAttribute
    {
        string[] _args;

        public OptionalAttArgsCuckooAttribute(string a, string b, string c = "pig") {
            _args = new[] { a, b, c };
        }
        
        public override void Call(ICall call) {
            base.Call(call);

            call.ReturnValue = _args;
        }
    }
}
