using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Impl
{
    public interface ICallArgChangeSink
    {
        void RegisterChange(int index);
    }
}
