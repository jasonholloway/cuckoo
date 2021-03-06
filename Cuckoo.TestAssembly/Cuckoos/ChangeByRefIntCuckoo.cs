﻿using Cuckoo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class ChangeByRefIntCuckooAttribute : CuckooAttribute
    {
        int _i;

        public ChangeByRefIntCuckooAttribute(int i) {
            _i = i;
        }

        public override void Call(ICall call) {
            call.CallInner();

            foreach(var arg in call.Args.Where(a => a.IsByRef && a.Type == typeof(int))) {
                arg.Value = _i;
            }
        }
    }
}
