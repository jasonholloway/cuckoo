using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Impl
{
    public abstract class CallBase<TInstance, TReturn> 
        : ICall, ICallArgChangeSink
    { 
        //public as these to be accessed by mOuter
        public ICallArg[] _callArgs;
        public TReturn _return;
        public ulong _argFlags = 0;
        
        protected IRoost _roost;
        protected TInstance _instance;
      
        ICuckoo[] _cuckoos;
        int _iNextCuckoo = 0;
        bool _hasInstance;
        bool _returnsValue;


        protected CallBase(
                    IRoost roost,
                    bool hasInstance,
                    bool returnsValue ) 
        {
            _roost = roost;
            _cuckoos = _roost.Cuckoos;
            _hasInstance = hasInstance;
            _returnsValue = returnsValue;
        }

        
        public void Prepare(ICallArg[] callArgs) {
            _callArgs = callArgs;

            foreach(var cuckoo in _cuckoos) {
                cuckoo.PreCall(this);
            }
        }

        public void Invoke(TInstance instance) {
            _instance = instance;

            InvokeNext();
        }


        void InvokeNext() {
            if(_iNextCuckoo < _cuckoos.Length) {
                var cuckoo = _cuckoos[_iNextCuckoo++];                
                cuckoo.Call(this);
            }
            else {
                InvokeFinal();
            }
        }

        protected abstract void InvokeFinal();


        #region IBeforeCall

        bool IBeforeCall.HasInstance {
            get { return _hasInstance; }
        }

        bool IBeforeCall.HasReturnValue {
            get { return _returnsValue; }
        }

        IRoost IBeforeCall.Roost {
            get { return _roost; }
        }

        MethodBase IBeforeCall.Method {
            get { return _roost.Method; }
        }

        ICallArg[] IBeforeCall.Args {
            get { return _callArgs; }
        }

        #endregion


        #region ICall

        object ICall.Instance {
            get { return _instance; }
        }

        object ICall.ReturnValue {
            get {
                return _return;
            }
            set {
                if(!typeof(TReturn).IsAssignableFrom(value.GetType())) {
                    throw new InvalidCastException(string.Format("Method {0} can't return value of type {1}!", _roost.Method.Name, value.GetType().Name));
                }

                _return = (TReturn)value;
            }
        }

        void ICall.CallInner() {
            InvokeNext();
        }

        #endregion


        #region ICallArgChangeSink

        void ICallArgChangeSink.RegisterChange(int index) {
            _argFlags |= (1LU << index);
        }

        #endregion
    }
}
