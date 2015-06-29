using Cuckoo.Impl;
using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{

    //public class GenRet
    //{

    //    public T MethodWithGenericResult2<T>(int a) {
    //        var call = new DummyCall<T>();

    //        call.PreInvoke();

    //        call.InvokeNext();

    //        var result = call._return;

    //        return result;
    //    }



    //    class DummyCall<TRet> : CallBase<GenRet, TRet>
    //    {
    //        public DummyCall()
    //            : base(null, null, null) {
    //            //...
    //        }

    //        protected override void InvokeInnerMethod() {

    //            var arg = new CallArg<TRet>(null, default(TRet));


    //            var v = arg._value;

    //            //throw new NotImplementedException();
    //        }
    //    }



    //}


    public class GenericArgs
    {
        [BareCuckoo]
        public int MethodWithGenericArgs<A, B>(A a, B b) {
            return 999;
        }

        [BareCuckoo]
        public T MethodWithGenericResult<T>(int a) {
            return default(T);
        }

        [BareCuckoo]
        [AddingCuckoo(10)]
        [DeductingCuckoo(20)]
        public int TreblyCuckooedMethodWithGenericArgs<A, B, C>(A a, B b, C c) {
            return 100;
        }


        [AddingCuckoo(1)]
        public List<int> MethodReturningList() {
            return new List<int>(new[] { 1, 2, 3, 4, 5 });
        }


    }

}
