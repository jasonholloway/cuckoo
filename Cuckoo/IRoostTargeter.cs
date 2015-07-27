using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo
{
    public interface IRoostTargeter
    {
        IEnumerable<RoostTarget> TargetRoosts(Assembly assembly);    
    }
    
}
