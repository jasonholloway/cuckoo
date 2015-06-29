using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public struct Structs {

        public Structs(int number) {
            Number = number;
        }

        public int Number;

        [AddingCuckoo(100)]
        public int GetNumber() {     
            return Number;
        }

        [ReturnInstanceCuckoo]
        public object GetInstance() {
            return null;
        }



    }
}
