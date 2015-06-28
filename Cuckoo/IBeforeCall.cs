using System.Reflection;

namespace Cuckoo
{
    public interface IBeforeCall
    {
        bool HasInstance { get; }
        bool HasReturnValue { get; }

        IRoost Roost { get; }
        MethodBase Method { get; }
        ICallArg[] Args { get; }
    }

}
