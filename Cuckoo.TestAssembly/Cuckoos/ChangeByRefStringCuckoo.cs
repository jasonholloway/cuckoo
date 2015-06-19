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
    public class ChangeByRefStringCuckooAttribute : CuckooAttribute
    {
        string _s;

        public ChangeByRefStringCuckooAttribute(string s) {
            _s = s;
        }

        public override void OnInvokeke(ICall call) {
            call.CallInner();

            foreach(var arg in call.Args.Where(a => a.Type.IsByRef && a.Type.GetElementType() == typeof(string))) {
                arg.Value = _s;
            }
        }
    }
}
