using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo
{
    public interface IRoostPicker
    {
        IEnumerable<RoostSpec> PickRoosts(Assembly assembly);    
    }
    

}
