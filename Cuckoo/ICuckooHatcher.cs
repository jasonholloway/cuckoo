using System.Collections.Generic;

namespace Cuckoo
{
    public interface ICuckooHatcher
    {
        IEnumerable<ICuckoo> HatchCuckoos(IRoost roost);
    }
}
