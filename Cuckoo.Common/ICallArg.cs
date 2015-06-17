using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Refl = System.Reflection;


namespace Cuckoo.Common
{
    public interface ICallArg
    {
        ParameterInfo Parameter { get; }
        string Name { get; }
        Type Type { get; }
        object Value { get; set; }
    }

}
