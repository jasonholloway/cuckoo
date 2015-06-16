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

    //internal class CallArgInfo
    //{
    //    public FieldReference Field { get; private set; }
    //    public ParameterDefinition MethodParam { get; private set; }
    //    public ParameterReference CallParam { get; private set; }
    //    public bool IsByRef { get; private set; }

    //    public CallArgInfo(
    //        FieldReference field, 
    //        ParameterDefinition methodParam,
    //        ParameterReference callParam) 
    //    {
    //        Field = field;
    //        MethodParam = methodParam;
    //        IsByRef = MethodParam.ParameterType.IsByReference;
    //        CallParam = callParam;
    //    }
    //}

    internal class CallInfo
    {
        public TypeReference Type { get; private set; }
        public MethodReference CtorMethod { get; private set; }
        public MethodReference PostCuckooMethod { get; private set; }
        public FieldReference ReturnField { get; private set; }
        //public CallArgInfo[] Args { get; private set; }
        public bool RequiresInstanciation { get; private set; }
        public bool ReturnsValue { get; private set; }

        public CallInfo(
            TypeReference type, 
            MethodReference ctorMethod, 
            MethodReference postCuckooMethod,
            FieldReference returnField)
            //IEnumerable<CallArgInfo> args) 
        {
            Type = type;
            CtorMethod = ctorMethod;
            PostCuckooMethod = postCuckooMethod;
            ReturnField = returnField;
            //Args = args.ToArray();
            RequiresInstanciation = Type.HasGenericParameters;
            ReturnsValue = ReturnField != null;
        }

        public CallInfo Instanciate(IEnumerable<TypeReference> genArgs) {            
            var tCallRef = Type.MakeGenericInstanceType(genArgs.ToArray());
            var mCtorRef = tCallRef.ReferenceMethod(m => m.IsConstructor);

            return new CallInfo(tCallRef,
                                        mCtorRef,
                                        tCallRef.ReferenceMethod(PostCuckooMethod.Name),
                                        ReturnsValue 
                                            ? tCallRef.ReferenceField(ReturnField.Name) 
                                            : null//,
                                        //Args.Select(a => new CallArgInfo(
                                        //                        tCallRef.ReferenceField(a.Field.Name),
                                        //                        a.MethodParam,
                                        //                        mCtorRef.Parameters[a.CallParam.Index]
                                        //                        ))
                                        );
        }

    }



}
