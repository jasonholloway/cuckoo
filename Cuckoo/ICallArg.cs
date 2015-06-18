using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Cuckoo
{
    public interface ICallArg
    {
        ParameterInfo Parameter { get; }
        string Name { get; }
        Type Type { get; }
        object Value { get; set; }
    }

}
