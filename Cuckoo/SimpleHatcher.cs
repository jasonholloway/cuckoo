using System;
using System.Collections.Generic;

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
