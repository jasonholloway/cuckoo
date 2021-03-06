﻿using Cuckoo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class CtorArgChangingCuckooAttribute : CuckooAttribute
    {

        public override void PreCall(ICallArg[] callArgs) {
            foreach(var stringArg in callArgs.Where(a => a.Type == typeof(string))) {
                stringArg.Value = "Chirp!";
            }

            foreach(var intArg in callArgs.Where(a => a.Type == typeof(int))) {
                intArg.Value = 77;
            }
        }


        public override void Call(ICall call) {
            foreach(var stringArg in call.Args.Where(a => a.Type == typeof(string))) {
                stringArg.Value = "Woof!";
            }

            foreach(var intArg in call.Args.Where(a => a.Type == typeof(int))) {
                intArg.Value = 99;
            }

            call.CallInner();
        }


    }
}
