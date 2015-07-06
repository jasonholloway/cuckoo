using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class AttTypeArgCuckooAttribute : CuckooAttribute
    {
        Type _type;
        bool _returnsString = false;

        public AttTypeArgCuckooAttribute(Type type) {
            _type = type;
        }


        public override void Init(IRoost roost) {
            var method = roost.Method as MethodInfo;

            if(method != null) {
                _returnsString = method.ReturnType == typeof(string);
            }
        }

        public override void Call(ICall call) {
            base.Call(call);

            if(_returnsString) {
                call.ReturnValue = _type.FullName;
            }
        }
    }
}
