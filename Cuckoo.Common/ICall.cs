using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Common
{
    public interface ICall
    {
        object Instance { get; }
        MethodInfo Method { get; }
        ICallArg[] Args { get; }
        object ReturnValue { get; set; }

        void CallInner();
    }



    public class SomeCuckooAttribute : CuckooAttribute 
    {
        public SomeCuckooAttribute(string name) {
            //...
        }

        public override void Init(MethodInfo method) {
            //...
        }

        public override void Usurp(ICall call) {
            throw new NotImplementedException();
        }
    }



    public class BeforeInstance
    {
        [SomeCuckoo("hello!")]
        public string Method(int num) {
            var s = string.Format("Yo {0}!", num.ToString());
            return s;
        }
    }



    //each call factory 

    public abstract class CallFactory
    {

    }


    /*
    public class AfterInstance
    {
        static CallSite _callSite;

        [SomeCuckoo("hello!")]
        public string Method(int num) {
            var call = new _MethodCall(this, num);

            _callSite.Usurper.Usurp(call);
                
            return (string)call.ReturnValue; //only if return needed...
        }

        [SomeCuckoo("hello!")]
        public string _Usurped_Method(int num) {
            var s = string.Format("Yo {0}!", num.ToString());
            return s;
        }

        
        //create a special call class for each call site
        class _MethodCall : ICall {

            static MethodInfo _method = null;
            AfterInstance _instance;
            string _return;
            int _arg0;
            object[] _args;

            public _MethodCall(AfterInstance instance, int arg0) {
                _instance = instance;
                _arg0 = arg0;
                _args = new object[] { arg0 };
            }

            void ICall.CallInner() {
                //if we have a return...
                _return = _instance._Usurped_Method(_arg0);
            }

            object ICall.Instance {
                get { return _instance; }
            }

            MethodInfo ICall.Method {
                get { return _method; }
            }

            object[] ICall.Args {
                get { return _args; }
            }

            object ICall.ReturnValue {
                get {
                    return _return; //watch out for boxing here!
                }
                set {
                    _return = (string)value; //watch out for boxing here!
                }
            }

            public string ReturnValue {
                get { return _return; }
            }


        }

    }
*/


}
