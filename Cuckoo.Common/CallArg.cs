using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Refl = System.Reflection;


namespace Cuckoo.Common
{


    public class CallArg
    {
        ParameterInfo _paramInfo;
        object _value;
        bool _isPristine;

        public CallArg(ParameterInfo paramInfo, object value) {
            _paramInfo = paramInfo;
            _value = value;
            _isPristine = false;
        }

        public ParameterInfo Parameter {
            get { return _paramInfo; }
        }

        public string Name {
            get { return _paramInfo.Name; }
        }

        public Type Type {
            get { return _paramInfo.ParameterType; }
        }

        public bool IsPristine {
            get { return _isPristine; }
        }

        public object Value {
            get {
                return _value;
            }
            set {
                if(value != _value) {
                    _value = value;
                    _isPristine = false;
                }
            }
        }

    }
}
