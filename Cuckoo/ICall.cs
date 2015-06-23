using System.Reflection;

namespace Cuckoo
{
    public interface ICall
    {
        bool HasInstance { get; }
        bool HasReturnValue { get; }

        object Instance { get; }
        MethodBase Method { get; }
        ICallArg[] Args { get; }
        object ReturnValue { get; set; }

        void CallInner(); //Should check phase of call!


        //Call has to delegate to inner method via CallInner, or indeed to next cuckoo...
        //the meaning of CallInner changes for each cuckoo.

        //Each call to h
    }

}
