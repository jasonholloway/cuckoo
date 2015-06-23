using Mono.Cecil;
using Mono.Cecil.Rocks;
using Cuckoo.Fody.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
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
            public MethodWeaver.ArgSpec MethodArg { get; private set; }
                       

            public static ArgSpec[] CreateAll(
                                        WeaveContext ctx, 
                                        ScopeTypeMapper types, 
                                        MethodWeaver.ArgSpec[] methodArgSpecs ) 
            {
                return methodArgSpecs
                        .Select(s => {
                            var R = ctx.RefMap;

                            var tCallArgRef = R.CallArg_Type.MakeGenericInstanceType(
                                                                s.Param.ParameterType.GetElementType()
                                                                );

                            return new ArgSpec() { 
                                        Index = s.Index,
                                        Type = s.Type,
                                        IsByRef = s.IsByRef,
                                        CallArg_Type = tCallArgRef,
                                        CallArg_fValue = tCallArgRef.ReferenceField(R.CallArg_fValue.Name),
                                        MethodArg = s
                                        };
                        })
                        .ToArray();
            }
        }
    }
}
