﻿using Cuckoo;
using Cuckoo.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class ArgChangingCuckooAttribute : CuckooAttribute
    {

        //OnBeforeInvoke

        
        public override void OnCall(ICall call) {
            foreach(var stringArg in call.Args.Where(a => a.Type == typeof(string))) {
                stringArg.Value = "Growl";
            }

            foreach(var intArg in call.Args.Where(a => a.Type == typeof(int))) {
                intArg.Value = 13;
            }

            call.CallInner();
        }
    }
}
