using System.Reflection;

namespace Cuckoo
{
    public interface IRoost
    {
        Method Method { get; }
        ICuckoo[] Cuckoos { get; }
    }
}
