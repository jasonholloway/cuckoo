using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cuckoo.TestAssembly.Cuckoos;

namespace Cuckoo.TestAssembly
{
    public class Atts
    {

        [OptionalCtorArgsCuckoo(99)]
        public string MethodWithOptArgAttribute() {
            return "GAH";
        }



    }
}
