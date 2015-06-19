using System.Reflection;

namespace Cuckoo
{
    public interface IRoost
    {
        MethodBase Method { get; }
        ICuckoo[] Cuckoos { get; }
    }
}
