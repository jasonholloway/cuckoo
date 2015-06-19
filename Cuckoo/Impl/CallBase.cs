using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Impl
{
    public abstract class CallBase : ICall
    {
        public object Instance {
            get { throw new NotImplementedException(); }
        }

        public MethodBase Method {
            get { throw new NotImplementedException(); }
        }

        public ICallArg[] Args {
            get { throw new NotImplementedException(); }
        }

        public object ReturnValue {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public void CallInner() {
            throw new NotImplementedException();
        }


        public void Proceed() {
            throw new NotImplementedException();
        }
    }
}
