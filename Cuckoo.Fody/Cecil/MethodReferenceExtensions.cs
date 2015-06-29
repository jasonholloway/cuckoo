using Mono.Cecil;

namespace Cuckoo.Fody.Cecil
{
    public static class MethodReferenceExtensions
    {

        public static GenericInstanceMethod MakeGenericInstanceMethod(this MethodReference @this, params TypeReference[] args) {
            var mInst = new GenericInstanceMethod(@this);

            foreach(var arg in args) {
                mInst.GenericArguments.Add(arg);
            }

            return mInst;
        }


        public static bool ReturnsValue(this MethodReference @this) {
            return @this.ReturnType != @this.Module.TypeSystem.Void;
        }

    }
}
