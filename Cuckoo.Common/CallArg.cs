using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Refl = System.Reflection;


namespace Cuckoo.Common
{

    public abstract class CallArg2
    {
        public ParameterInfo Parameter { get; protected set; }
        public string Name { get; protected set; }
        public Type Type { get; protected set; }

        public abstract object Value { get; set; }
    }

    public class TypedCallArg<TVal> : CallArg2
    {
        TVal _value;

        public TypedCallArg(TVal value) {
            _value = value;
        }

        public TVal TypedValue {
            get {
                throw new NotImplementedException();
            }
            set {
                //...
            }
        }

        public override object Value {
            get {
                throw new NotImplementedException();
            }
            set {
                //check is byref here
                //...

                //check correct type here
                //...

                throw new NotImplementedException();
            }
        }

    }


    public class CallArg : ICallArgStatus 
    {
        ParameterInfo _paramInfo;
        object _value;
        bool _hasChanged = false;

        public CallArg(ParameterInfo paramInfo, object value) {
            _paramInfo = paramInfo;
            _value = value;
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

        public object Value {
            get {
                return _value;
            }
            set {
                if(value != _value) {
                    _value = value;
                    _hasChanged = true;
                }
            }
        }

        bool ICallArgStatus.HasChanged {
            get { return _hasChanged; }
        }
    }
}
