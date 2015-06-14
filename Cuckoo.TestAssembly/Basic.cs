﻿using Cuckoo.Common;
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


        [ReturnChangingCuckoo("CHANGED!")]
        public string MethodWithChangeableReturn() {
            return "Blahdy blah blah";
        }


        [AddingCuckoo(8)]
        [DeductingCuckoo(10)]
        public int MethodReturnsInt() {
            return 13;
        }


        [ArgChangingCuckoo]
        public string MethodReturnsStrings(int a, string b, string c, float d, string e) {
            return string.Format("{0}! {1}! {2}!", b, c, e);
        }


        [ReturnChangingCuckoo("BLAH")]
        [ReturnChangingCuckoo2("Wow!")]
        public string MethodWithTwoCuckoos(string s, int b) {
            return s;
        }



        public void Dummy2() { }
    }
}