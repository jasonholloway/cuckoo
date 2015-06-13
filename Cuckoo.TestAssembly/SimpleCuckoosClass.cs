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
    public class SimpleCuckoosClass
    {


        class Nested
        {
            int _a;
            string _b;
            object[] _r;

            public Nested(int a, string b) {
                _a = a;
                _b = b;

                if(_r != null) {
                    _a = 99;
                }

                _b = "88";
            }

            public void Blah<T>(T t) {

                object i = 3;

                int[] rInts = new int[10];

                rInts[5] = (int)i;



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


        [SimpleCuckoo]
        public int MethodWithGenericArgs<A, B>(A a, B b) {
            return 999;
        }


        [SimpleCuckoo]
        public T MethodWithGenericResult<T>(int a) {
            return default(T);
        }

        [SimpleCuckoo]
        [AddingCuckoo(10)]
        [DeductingCuckoo(20)]
        public int TreblyCuckooedMethodWithGenericArgs<A, B, C>(A a, B b, C c) {
            return 100;
        }




        /*
        [SimpleCuckoo]
        public T GenericMethod<T>() {
            return default(T);
        }
        */



        public void Dummy2() { }
    }
}
