using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public struct TestStruct {

        public TestStruct(int number) {
            _number = number;
        }

        public int _number;

        [AddingCuckoo(100)]
        public int GetNumber() {     
            return _number;
        }

        [ReturnInstanceCuckoo]
        public object GetInstance() {
            return null;
        }

        public int Number {
            [BareCuckoo]
            set { _number = value; }
        }




    }
}
