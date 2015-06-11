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
        public void MethodWithFieldCuckoo() {
            string blah = "grrrowl";
        }

        [SimpleCuckoo]
        public string MethodReturnsString() {
            return "Hello from down below!";
        }


        public void Dummy2() { }
    }
}
