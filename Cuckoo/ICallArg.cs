using System;
using System.Reflection;

namespace Cuckoo
{
    public interface ICallArg
    {
        string Name { get; }
        object Value { get; set; }
        Type Type { get; }
        bool IsByRef { get; }
        ParameterInfo Parameter { get; }
    }

    public interface ICallArg<TValue> : ICallArg
    {
        TValue TypedValue { get; set; }
    }
}
