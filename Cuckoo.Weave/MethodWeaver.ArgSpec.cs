using Cuckoo.Weave.Cecil;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;
using System.Linq;

namespace Cuckoo.Weave
{
    internal partial class MethodWeaver
    {
        internal class ArgSpec
        {
            private ArgSpec() { }
            
            public int Index { get; private set; }
            public ParameterReference OuterParam { get; private set; }
            public TypeReference Type { get; private set; }
            public bool IsByRef { get; private set; }

            public TypeReference CallArg_Type { get; private set; }
            public MethodReference CallArg_mCtor { get; private set; }
            public FieldReference CallArg_fValue { get; private set; }


            public static ArgSpec[] CreateAll(
                                        WeaveContext ctx,
                                        IEnumerable<ParameterReference> outerParams) 
            {
                var R = ctx.RefMap;
                int iParam = 0;

                return outerParams
                            .Select(p => {
                                var argType = p.ParameterType.IsByReference
                                                ? p.ParameterType.GetElementType()
                                                : p.ParameterType;

                                var tCallArgRef = R.CallArg_Type.MakeGenericInstanceType(argType);

                                return new ArgSpec() {
                                    Index = iParam++,
                                    OuterParam = p,
                                    Type = argType,
                                    IsByRef = p.ParameterType.IsByReference,
                                    CallArg_Type = tCallArgRef,
                                    CallArg_mCtor = tCallArgRef.ReferenceMethod(m => m.IsConstructor),
                                    CallArg_fValue = tCallArgRef.ReferenceField(R.CallBase_fReturn.Name)
                                };
                            })
                            .ToArray();
            }
        }
    }
}
