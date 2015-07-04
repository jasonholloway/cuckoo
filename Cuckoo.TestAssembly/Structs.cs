using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{

    //NEED SOME KIND OF AGENT


    public class StructRunner : MarshalByRefObject
    {
        public int GetNumber(int i) {
            return new TestStruct(i).GetNumber();
        }

        public object GetInstance(int i) {
            return new TestStruct(i).GetInstance();
        }

        public int SetAndRetrieveNumber(int i) {
            return new TestStruct(9631) { Number = i }._number;
        }
    }

    [Serializable]
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
