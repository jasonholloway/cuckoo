using Cuckoo.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericExample
{
    internal class ByRefs
    {


        class Call<TRet> : CallBase<ByRefs, TRet>
        {
            public Call() 
                : base(null, null, null, true, true) { }

            protected override void InvokeFinal() {
                //...
            }
        }




        public T GenTest<T>(int a) {
            var call = new Call<T>();

            call.PreInvoke();

            call.InvokeNext();

            var r = call._return;

            return r;
        }



        public float ReturnsFloat() {
            return 13F;
        }

        public object ReturnsObject() {
            return null;
        }


        public void CallTest() {

            CallBase<ByRefs, int> callBase = null;

            var r = callBase._return;

            if(r == 99) {
                CallTest();
            }

        }






        public void ByRefMethod(int a, out int b, out string c, int d = 99, object o = null) {
            b = 99;
            c = "BLAH";
        }



    }
}
