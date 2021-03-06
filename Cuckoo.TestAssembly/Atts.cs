﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cuckoo.TestAssembly.Cuckoos;
using Cuckoo.AnotherTestAssembly;

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


        [AttTypeArgCuckoo(typeof(Atts))]
        public string TypeArgInAttCtor() {
            return "";
        }

        [AttArgsCuckoo(1, 'a', 0xFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF, 1F, 1D, "blah", typeof(Atts))]
        public object[] VariousAttArgs() {
            return new object[0];
        }

        [AttPropsCuckoo(Byte = 1, Char = 'a', Double = 1D, Float = 1F, Int = 1, Long = 0xFFFFFFFFFF, String = "brap", Type = typeof(DistantCuckooAttribute), UInt = 0xFFFFFFFF, ULong = 0xFFFFFFFFFFFFFFFF)]
        public object[] VariousAttProps() {
            return new object[0];
        }

        [AttArgsCuckoo(123, 123, 123, 123)]
        public object[] AttArgsByParamsArray() {
            return new object[0];
        }

        [OptionalAttArgsCuckoo("plop", "pork")]
        public string[] OptionalAttArgs() {
            return new string[0];
        }

    }
}
