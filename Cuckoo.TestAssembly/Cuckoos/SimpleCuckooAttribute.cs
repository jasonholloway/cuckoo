﻿using Cuckoo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class SimpleCuckooAttribute : CuckooAttribute
    {
        public override void Init(MethodInfo method) {
            //...
        }
        public override void Usurp(ICall call) {
            throw new NotImplementedException();
        }
    }


}
