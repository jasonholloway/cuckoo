using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Impl
{
    public class CallArg<TVal> : ICallArg<TVal>
    {
        public TVal _value;

        public CallArg(ParameterInfo param, TVal value) {
            _value = value;
            Parameter = param;
        }


        public ParameterInfo Parameter { get; private set; }


        public string Name {
            get { return Parameter.Name; }
        }

        public Type Type {
            get { return Parameter.ParameterType.GetElementType(); }
        }

        public bool IsByRef {
            get { return Parameter.ParameterType.IsByRef; }
        }



        object ICallArg.Value {
            get {
                return _value;
            }
            set {
                //check is byref here - depends on current phase of usurpation
                //...

                if(!typeof(TVal).IsAssignableFrom(value.GetType())) {
                    throw new InvalidCastException(string.Format("CallArg of {0} can't be assigned value of {1}", typeof(TVal).Name, value.GetType().Name));
                }

                _value = (TVal)value;
            }
        }

        TVal ICallArg<TVal>.TypedValue {
            get {
                return _value;
            }
            set {
                _value = value;
            }
        }

    }

}
