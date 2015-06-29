using Cuckoo;
using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class Basic
    {
        public void Dummy1() { }

        [BareCuckoo]
        public void SimpleMethod() {
            //...
        }

        [BareCuckoo]
        public int SimpleMethodWithReturn() {
            return 13;
        }



        [BareCuckoo]
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


        [BareCuckoo]
        public string MethodReturnsString() {
            return "Hello from down below!";
        }


        [ReturnChangingCuckoo("CHANGED!")]
        public string MethodWithChangeableReturn() {
            return "Blahdy blah blah";
        }


        [AddingCuckoo(8)]
        [DeductingCuckoo(10)]
        public int MethodReturnsInt() {
            return 13;
        }



        [ReturnChangingCuckoo("BLAH")]
        [ReturnChangingCuckoo2("Wow!")]
        public string MethodWithTwoCuckoos(string s, int b) {
            return s;
        }



        public void Dummy2() { }

        [BareCuckoo]
        [ArgCuckoo("aaaa")]
        public void VoidMethod() {
            //...
        }





        public abstract class CtorClassBase
        {
            public int BaseValue { get; private set; }

            public CtorClassBase(int baseValue) {
                BaseValue = baseValue;
            }
        }

        public class CtorClass : CtorClassBase
        {
            public int DerivedValue { get; private set; }

            [BareCuckoo]
            [CtorArgChangingCuckoo]
            public CtorClass(int baseValue, int derivedValue) 
                : base(baseValue * 100) 
            {
                DerivedValue = derivedValue;
            }


            [CheckInstanceInPlaceCuckoo]
            public CtorClass(int a, int b, int c)
                : base(1) {
                DerivedValue = a;
            }


        }



    }
}
