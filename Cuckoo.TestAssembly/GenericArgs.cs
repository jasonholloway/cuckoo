using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    class GenericArgs
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


    }

}
