using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class ClassWithStaticCtor : MarshalByRefObject
    {

        static int _aNum = 8;

        static ClassWithStaticCtor() {
            _aNum = 89;
        }

        public ClassWithStaticCtor() {
        }

        [BareCuckoo]
        public string Hello(string recipient1, string recipient2) {
            string greeting = "Yip Yip!";

            return string.Format("{2} {0} and {1}!", recipient1, recipient2, greeting);
        }



    }
}
