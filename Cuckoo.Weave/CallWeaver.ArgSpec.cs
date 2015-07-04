using Cuckoo.Weave.Cecil;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Linq;

namespace Cuckoo.Weave
{
    internal partial class CallWeaver
    {
        internal class ArgSpec
        {
            private ArgSpec() { }
            
            public bool IsByRef { get; private set; }
            public int Index { get; private set; }
            public TypeReference Type { get; private set; }

            public TypeReference CallArg_Type { get; private set; }
            public FieldReference CallArg_fValue { get; private set; }
            public RoostWeaver.ArgSpec MethodArg { get; private set; }
                       

            public static ArgSpec[] CreateAll(
                                        WeaveContext ctx, 
                                        ScopedTypeSource types, 
                                        RoostWeaver.ArgSpec[] methodArgSpecs ) 
            {
                return methodArgSpecs
                        .Select(ms => {
                            var R = ctx.RefMap;
                            
                            var argType = types.Map(ms.Type);

                            var tCallArgRef = R.CallArg_Type
                                                    .MakeGenericInstanceType(argType); 

                            return new ArgSpec() { 
                                        Index = ms.Index,
                                        Type = argType,
                                        IsByRef = ms.IsByRef,
                                        CallArg_Type = tCallArgRef,
                                        CallArg_fValue = tCallArgRef.ReferenceField(R.CallArg_fValue.Name),
                                        MethodArg = ms
                                        };
                        })
                        .ToArray();
            }
        }
    }
}
