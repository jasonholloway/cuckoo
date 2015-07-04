using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cuckoo.TestAssembly.Cuckoos;

namespace Cuckoo.TestAssembly
{
    public class Atts : MarshalByRefObject
    {

        [OptionalCtorArgsCuckoo(99)]
        public string MethodWithOptArgAttribute() {
            return "GAH";
        }


        [ArgChangingCuckoo]
        private string PrivateMethod(string s) {
            return s;
        }

        public string PrivateMethodRunner(string s) {
            return PrivateMethod(s);
        }

    }
}
