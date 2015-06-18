using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo
{
    public interface ICall
    {
        object Instance { get; }
        MethodInfo Method { get; }
        ICallArg[] Args { get; }
        object ReturnValue { get; set; }

        void CallInner();
    }

}
