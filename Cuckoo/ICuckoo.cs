
namespace Cuckoo
{
    public interface ICuckoo
    {
        void OnRoost(IRoost roost);
        void OnBeforeCall(ICall call);
        void OnCall(ICall call);
    }

}
