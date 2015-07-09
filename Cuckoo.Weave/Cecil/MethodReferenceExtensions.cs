using Mono.Cecil;
using System.Linq;

namespace Cuckoo.Weave.Cecil
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


        public static TypeReference[] ResolveParamTypes(this MethodReference @this) 
        {
            if(@this is GenericInstanceMethod) {
                var instMethod = (GenericInstanceMethod)@this;

                return @this.Parameters
                                .Select(p => {
                                    var paramType = p.ParameterType;

                                    if(paramType is GenericParameter) {
                                        var genParam = (GenericParameter)paramType;

                                        switch(genParam.Type) {
                                            case GenericParameterType.Method:
                                                return instMethod.GenericArguments[genParam.Position];

                                            case GenericParameterType.Type:
                                                return ((GenericInstanceType)instMethod.DeclaringType)
                                                                        .GenericArguments[genParam.Position];
                                        }
                                    }

                                    return paramType;
                                })
                                .ToArray();
            }
            else {
                return @this.Parameters
                                .Select(p => p.ParameterType)
                                .ToArray();
            }
        }


    }
}
