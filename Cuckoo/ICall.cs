using System.Reflection;

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
