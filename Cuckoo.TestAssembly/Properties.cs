using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class Properties : MarshalByRefObject 
    {


        public int Value = 25;

        public int Prop {
            [DeductingCuckoo(8)]
            [BareCuckoo]
            get {
                return Value;
            }
            [ArgChangingCuckoo]
            set {
                Value = value;
            }
        }
        
    }
}
