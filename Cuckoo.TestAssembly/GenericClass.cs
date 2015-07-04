using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class GenericClass<A, B> : MarshalByRefObject
    {

        [BareCuckoo]
        public int MethodInGenericClass(int i) {
            A a = default(A);
            return 12345;
        }


        [BareCuckoo]
        [DeductingCuckoo(100)]
        public int MethodWithGenericArgsInGenericClass<C, D>(C c, D d) {
            var cc = c;
            var dd = d;
            return 987;
        }



    }

}
