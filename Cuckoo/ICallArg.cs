using System;
using System.Reflection;

namespace Cuckoo
{
    public interface ICallArg
    {
        string Name { get; }
        object Value { get; set; }
        Type ValueType { get; }
        bool IsByRef { get; }
        ParameterInfo Parameter { get; }
    }

    public interface ICallArg<TValue> : ICallArg
    {
        TValue TypedValue { get; set; }
    }
}
