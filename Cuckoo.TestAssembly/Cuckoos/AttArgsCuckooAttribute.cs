using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class AttArgsCuckooAttribute : CuckooAttribute
    {
        bool _returnsObjArray = false;
        object[] _args;

        public AttArgsCuckooAttribute(params object[] args) {
            _args = args;
        }

        public AttArgsCuckooAttribute(
                byte b,
                char c,
                int a,
                uint ui,
                long l,
                ulong ul,
                float f,
                double d,
                string s,
                Type t) 
        {
            _args = new object[] { 
                b, c, a, ui, l, ul, f, d, s, t
            };
        }

        public override void Init(IRoost roost) {
            var method = roost.Method as MethodInfo;

            if(method != null) {
                _returnsObjArray = method.ReturnType == typeof(object[]);
            }
        }

        public override void Call(ICall call) {
            base.Call(call);

            if(_returnsObjArray) {
                call.ReturnValue = (object[])_args;
            }
        }
    }
}
