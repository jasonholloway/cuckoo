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
    public class ChangeByRefIntCuckooAttribute : CuckooAttribute
    {
        int _i;

        public ChangeByRefIntCuckooAttribute(int i) {
            _i = i;
        }

        public override void OnInvoke(ICall call) {
            call.CallInner();

            foreach(var arg in call.Args.Where(a => a.Type.IsByRef && a.Type.GetElementType() == typeof(int))) {
                arg.Value = _i;
            }
        }
    }
}
