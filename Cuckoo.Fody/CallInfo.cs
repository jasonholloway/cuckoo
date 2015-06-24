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

    internal class CallArgInfo
    {
        public FieldReference Field { get; private set; }
        public ParameterDefinition MethodParam { get; private set; }
        public ParameterReference CtorParam { get; private set; }
        public bool IsByRef { get; private set; }

        public CallArgInfo(
            FieldReference field,
            ParameterDefinition methodParam,
            ParameterReference ctorParam ) 
        {
            Field = field;
            MethodParam = methodParam;
            IsByRef = MethodParam.ParameterType.IsByReference;
            CtorParam = ctorParam;
        }
    }

    internal class CallInfo
    {
        public TypeReference Type { get; private set; }
        public MethodReference CtorMethod { get; private set; }
        public MethodReference PreInvokeMethod { get; private set; }
        public MethodReference InvokeNextMethod { get; private set; }
        public FieldReference ReturnField { get; private set; }
        public FieldReference ArgsField { get; private set; }
        public CallWeaver.ArgSpec[] Args { get; private set; }
        public bool RequiresInstanciation { get; private set; }
        public bool ReturnsValue { get; private set; }


        public CallInfo(
                    TypeReference type,
                    MethodReference ctorMethod,
                    MethodReference preInvokeMethod,
                    MethodReference invokeNextMethod,
                    FieldReference returnField,
                    FieldReference argsField,
                    CallWeaver.ArgSpec[] args ) 
        {
            Type = type;
            CtorMethod = ctorMethod;
            PreInvokeMethod = preInvokeMethod;
            InvokeNextMethod = invokeNextMethod;
            ReturnField = returnField;
            ArgsField = argsField;
            Args = args;
            RequiresInstanciation = Type.HasGenericParameters;
            ReturnsValue = ReturnField != null;
        }


        public CallInfo Instanciate(
                            IEnumerable<TypeReference> contGenArgs, 
                            IEnumerable<TypeReference> methodGenArgs ) 
        {         
            var tCallRef = Type.MakeGenericInstanceType(
                                        contGenArgs.Concat(methodGenArgs).ToArray());

            var tCallBaseRef = tCallRef.GetBaseType(); 

            var mod = Type.Module;

            return new CallInfo(tCallRef,
                                    tCallRef.ReferenceMethod(m => m.IsConstructor),
                                    tCallBaseRef.ReferenceMethod(PreInvokeMethod.Name),
                                    tCallBaseRef.ReferenceMethod(InvokeNextMethod.Name),
                                    ReturnsValue
                                        ? tCallBaseRef.ReferenceField(ReturnField.Name)
                                        : null,
                                    tCallBaseRef.ReferenceField(ArgsField.Name),
                                    Args
                                    );
        }

    }



}
