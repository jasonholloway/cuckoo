
namespace Cuckoo
{
    public interface ICuckoo
    {
        void Init(IRoost roost);
        void PreCall(IBeforeCall beforeCall);
        void Call(ICall call);
    }

}
