using System.Reflection;

namespace Cuckoo
{
    public interface IRoost
    {
        MethodInfo Method { get; }
        ICuckoo[] Cuckoos { get; }
    }
}
