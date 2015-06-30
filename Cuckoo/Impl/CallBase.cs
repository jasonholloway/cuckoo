using System;
using System.Diagnostics;
using System.Reflection;

namespace Cuckoo.Impl
{
    public abstract class CallBase<TInstance, TReturn> 
        : ICall, ICallArgChangeSink
    { 
        //public as these to be accessed by mOuter
        public ICallArg[] _callArgs;
        public TReturn _return;
        public ulong _argFlags = 0;
        public TInstance _instance;
        
        protected IRoost _roost;

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

        [DebuggerHidden]
        public void Prepare(ICallArg[] callArgs) {
            _callArgs = callArgs;

            foreach(var cuckoo in _cuckoos) {
                cuckoo.PreCall(callArgs);
            }
        }

        [DebuggerHidden]
        public void Invoke(TInstance instance) {
            _instance = instance;
            InvokeNext();
        }

        [DebuggerHidden]
        void InvokeNext() {
            if(_iNextCuckoo < _cuckoos.Length) {
                var cuckoo = _cuckoos[_iNextCuckoo++];                
                cuckoo.Call(this);
            }
            else {
                InvokeFinal();
            }
        }

        [DebuggerHidden]
        protected abstract void InvokeFinal();


        #region ICall

        bool ICall.HasInstance {
            get { return _hasInstance; }
        }

        bool ICall.HasReturnValue {
            get { return _returnsValue; }
        }

        IRoost ICall.Roost {
            get { return _roost; }
        }

        MethodBase ICall.Method {
            get { return _roost.Method; }
        }

        ICallArg[] ICall.Args {
            get { return _callArgs; }
        }

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

        [DebuggerHidden]
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
