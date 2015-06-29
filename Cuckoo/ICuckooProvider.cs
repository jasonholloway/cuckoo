using System.Collections.Generic;

namespace Cuckoo
{
    public interface ICuckooProvider
    {
        IEnumerable<ICuckoo> CreateCuckoos(IRoost roost);
    }
}
