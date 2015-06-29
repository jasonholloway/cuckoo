using System;
using System.Reflection;

namespace Cuckoo
{
    public interface IRoost
    {
        MethodBase Method { get; }
        ParameterInfo[] Parameters { get; }
        ICuckoo[] Cuckoos { get; }
    }
}
