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
        ParameterInfo _param;
        int _index;
        ICallArgChangeSink _changeSink;

        public TVal _value;


        public CallArg(
                    ParameterInfo param, 
                    int index, 
                    ICallArgChangeSink changeSink, 
                    TVal value ) 
        {
            _param = param;
            _index = index;
            _changeSink = changeSink;
            _value = value;
        }


        public ParameterInfo Parameter {
            get { return _param; }
        }

        public string Name {
            get { return Parameter.Name; }
        }

        public Type ValueType {
            get {
                return IsByRef
                        ? Parameter.ParameterType.GetElementType()
                        : Parameter.ParameterType;
            }
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
                _changeSink.RegisterChange(_index);
            }
        }


        TVal ICallArg<TVal>.TypedValue {
            get {
                return _value;
            }
            set {
                _value = value;
                _changeSink.RegisterChange(_index);
            }
        }

    }

}
