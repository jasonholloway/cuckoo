﻿using Cuckoo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class FieldCuckooAttribute : CuckooAttribute
    {
        public string Name { get; set; }

        public FieldCuckooAttribute() {
        }


        public override void Call(ICall call) {
            call.CallInner();
        }
    }


}
