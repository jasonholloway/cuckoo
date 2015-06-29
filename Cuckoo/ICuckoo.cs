
namespace Cuckoo
{
    public interface ICuckoo
    {
        void Init(IRoost roost);
        void PreCall(ICallArg[] callArgs);
        void Call(ICall call);
    }

}
