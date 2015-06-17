using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Common
{
    public interface IRoost
    {
        MethodInfo Method { get; }
        ICuckoo[] Cuckoos { get; }
    }
}
