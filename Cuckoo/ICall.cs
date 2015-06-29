using System.Reflection;

namespace Cuckoo
{
    public interface ICall
    {        
        IRoost Roost { get; }
        MethodBase Method { get; }
        ICallArg[] Args { get; }

        object Instance { get; }
        object ReturnValue { get; set; }

        bool HasInstance { get; }
        bool HasReturnValue { get; }

        void CallInner();
    }

}
