using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo
{
    public class SimpleHatcher<TCuckoo> : ICuckooHatcher
        where TCuckoo : ICuckoo, new()
    {
        public IEnumerable<ICuckoo> HatchCuckoos(IRoost roost) {
            return new ICuckoo[] { 
                new TCuckoo()
            };
        }
    }
}
