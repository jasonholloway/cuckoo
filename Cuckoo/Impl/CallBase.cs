using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Impl
{
    public abstract class CallBase<TInstance, TReturn> : ICall
    {
        protected IRoost _roost;
        protected TInstance _instance;
        public ICallArg[] _callArgs; //public as these to be accessed by mOuter
        public TReturn _return;

        ICuckoo[] _cuckoos;
        int _iNextCuckoo = 0;

        protected CallBase(
                    IRoost roost,
                    TInstance instance, 
                    ICallArg[] callArgs ) 
        {
            _roost = roost;
            _cuckoos = _roost.Cuckoos;
            _instance = instance;
            _callArgs = callArgs;
        }

        
        public void PreInvoke() {
            //call PreInvoke on each cuckoo...
        }

        public void InvokeNext() {
            if(_iNextCuckoo < _cuckoos.Length) {
                var cuckoo = _cuckoos[_iNextCuckoo++];
                
                cuckoo.OnCall(this);

                _iNextCuckoo--; //this is unnecessary
            }
            else {
                InvokeInnerMethod();
            }
        }

        protected abstract void InvokeInnerMethod(); //loads args onto stack etc


        #region ICall

        bool ICall.HasInstance {
            get { throw new NotImplementedException(); }
        }

        bool ICall.HasReturnValue {
            get { throw new NotImplementedException(); }
        }

        object ICall.Instance {
            get { return _instance; }
        }

        MethodBase ICall.Method {
            get { throw new NotImplementedException(); }
        }

        ICallArg[] ICall.Args {
            get { return _callArgs; }
        }

        object ICall.ReturnValue {
            get {
                return _return;
            }
            set {
                //check whether this is appropriate...
                _return = (TReturn)value;
            }
        }

        void ICall.CallInner() {
            InvokeNext();
        }

        #endregion

    }
}
