using Cuckoo;
using Cuckoo.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class ArgCuckooAttribute : CuckooAttribute
    {
        string _name;

        public ArgCuckooAttribute(string name) {
            _name = name;
        }


        public override void OnCall(ICall call) {
            call.CallInner();
        }
    }


}
