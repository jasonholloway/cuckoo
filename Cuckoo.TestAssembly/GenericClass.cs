using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    class GenericClass<A, B>
    {

        [SimpleCuckoo]
        public int MethodInGenericClass(int i) {
            A a = default(A);
            return 12345;
        }


        [SimpleCuckoo]
        [DeductingCuckoo(100)]
        public int MethodWithGenericArgsInGenericClass<C, D>(C c, D d) {
            var cc = c;
            var dd = d;
            return 987;
        }



    }

}
