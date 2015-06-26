using System;
using System.Reflection;


namespace Cuckoo
{
    public interface ICallArg
    {
        ParameterInfo Parameter { get; }
        string Name { get; }
        Type Type { get; }
        bool IsByRef { get; }
        object Value { get; set; }
    }

    public interface ICallArg<TValue> : ICallArg
    {
        TValue TypedValue { get; set; }
    }

}
