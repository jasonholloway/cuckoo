using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class AttArgsCuckooAttribute : CuckooAttribute
    {
        public AttArgsCuckooAttribute(
                byte b,
                char c,
                int a,
                uint ui,
                long l,
                ulong ul,
                float f,
                double d,
                string s,
                Type t) {

        }
        
        public override void Call(ICall call) {
            base.Call(call);
        }
    }
}
