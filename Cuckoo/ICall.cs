using System.Reflection;

namespace Cuckoo
{
    public interface ICall
    {
        object Instance { get; }
        MethodBase Method { get; }
        ICallArg[] Args { get; }
        object ReturnValue { get; set; }

        void CallInner(); //Should check phase of call!
        //void Proceed();   //Should check phase of call!
    }

}
