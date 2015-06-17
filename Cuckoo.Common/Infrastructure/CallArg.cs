using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Common.Infrastructure
{

    public class CallArg<TVal> : ICallArg
    {
        TVal _value;
        bool _hasChanged;

        public ParameterInfo Parameter { get; private set; }
        public string Name { get; private set; }
        public Type Type { get; private set; }

        public CallArg(ParameterInfo paramInfo, TVal value) {
            _value = value;

            this.Parameter = paramInfo;
            this.Name = paramInfo.Name;
            this.Type = paramInfo.ParameterType;
        }

        public TVal TypedValue {
            get { return _value; }
            set { _value = value; }
        }

        public object Value {
            get {
                return _value;
            }
            set {
                //check is byref here - depends on current phase of course
                //...

                if(!typeof(TVal).IsAssignableFrom(value.GetType())) {
                    throw new InvalidCastException(string.Format("CallArg of {0} can't be assigned value of {1}", typeof(TVal).Name, value.GetType().Name));
                }

                if(!_value.Equals(value)) {
                    _value = (TVal)value;
                    _hasChanged = true;
                }
            }
        }

        public bool HasChanged {
            get { return _hasChanged; }
        }

    }

}
