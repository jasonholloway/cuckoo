using Cuckoo.Common;
using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class SimpleCuckoosClass
    {


        class Nested
        {
            int _a;
            string _b;

            public Nested(int a, string b) {
                _a = a;
                _b = b;
            }

            public void Blah() {

                var r = new object[3];

                r[0] = 7;
                r[1] = 9;
                r[2] = 1;

                //...
            }
        }


        public void Dummy1() { }


        [SimpleCuckoo]
        public string MethodWithSimpleCuckoo(string recipient1, string recipient2) {
            string greeting = "Yip Yip!";

            return string.Format("{2} {0} and {1}!", recipient1, recipient2, greeting);
        }


        [ArgCuckoo("Yo Jason!")]
        public string MethodWithArgCuckoo(string recipient1, string recipient2) {
            string greeting = "Yip Yip!";

            return greeting;
        }

        [FieldCuckoo(Name = "Tony")]
        public string MethodWithFieldCuckoo() {
            string blah = "grrrowl";
            return blah;
        }

        [SimpleCuckoo]
        public string MethodReturnsString() {
            return "Hello from down below!";
        }


        public void Dummy2() { }
    }
}
