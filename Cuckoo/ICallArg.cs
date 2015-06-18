using System;
using System.Reflection;


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
