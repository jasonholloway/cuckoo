using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Common
{
    public interface ICallUsurper
    {
        void Init(MethodInfo methodInfo);
        void Usurp(ICall call);
    }


    class ExampleCallUsurper : ICallUsurper
    {
        MethodInfo _methodInfo;

        public void Init(MethodInfo methodInfo) {
            _methodInfo = methodInfo;
        }

        public void Usurp(ICall call) {
            Debug.Print(string.Join(", ", call.Args)); //can do stuff with args here

            call.CallInner(); //call inner - but only if we want to!

            call.ReturnValue = null; //can change return value here
        }
    } 

}
