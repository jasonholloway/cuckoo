﻿using Cuckoo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class AddingCuckooAttribute : CuckooAttribute
    {
        int _addendum;
        bool _returnsInt;


        public AddingCuckooAttribute(int addendum) {
            _addendum = addendum;
        }

        public override void Init(MethodInfo method) {
            _returnsInt = method.ReturnType == typeof(int);
        }

        public override void Usurp(ICall call) {
            call.CallInner();

            if(_returnsInt) {
                call.ReturnValue = (int)call.ReturnValue + _addendum;
            }
        }
    }
}
