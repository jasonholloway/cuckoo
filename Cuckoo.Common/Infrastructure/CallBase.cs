using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Common.Infrastructure
{
    public abstract class CallBase : ICall
    {
        public object Instance {
            get { throw new NotImplementedException(); }
        }

        public MethodInfo Method {
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
    }
}
