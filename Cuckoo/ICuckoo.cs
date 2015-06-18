using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo
{
    public interface ICuckoo
    {
        void OnRoost(IRoost roost);
        void OnCall(ICall call);
    }

}
