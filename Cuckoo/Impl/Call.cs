using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Impl
{
    public class Call<TInstance, TReturn> : ICall
    {
        public TInstance TypedInstance;
        public TReturn TypedReturnValue;
        public ICallArg[] CallArgs;

        public object Instance {
            get { return TypedInstance; }
        }

        public MethodBase Method {
            get { throw new NotImplementedException(); }
        }

        public ICallArg[] Args {
            get { throw new NotImplementedException(); }
        }

        public object ReturnValue {
            get {
                return TypedReturnValue;
            }
            set {
                TypedReturnValue = (TReturn)value;
            }
        }

        public void CallInner() {
            throw new NotImplementedException();
        }

        public bool HasInstance {
            get { throw new NotImplementedException(); }
        }

        public bool HasReturnValue {
            get { throw new NotImplementedException(); }
        }
    }
}
