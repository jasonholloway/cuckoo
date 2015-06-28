using System.Reflection;

namespace Cuckoo
{
    public interface ICall : IBeforeCall
    {
        object Instance { get; }
        object ReturnValue { get; set; }

        void CallInner();
    }

}
