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
        public MethodReference PreDispatchMethod { get; private set; }
        public MethodReference DispatchMethod { get; private set; }
        public FieldReference ReturnField { get; private set; }
        public FieldReference ArgsField { get; private set; }
        public CallWeaver.ArgSpec[] Args { get; private set; }
        public bool RequiresInstanciation { get; private set; }
        public bool ReturnsValue { get; private set; }

        public CallInfo(
            TypeReference type, 
            MethodReference ctorMethod, 
            MethodReference preDispatchMethod,
            MethodReference dispatchMethod,
            FieldReference returnField,
            FieldReference argsField,
            CallWeaver.ArgSpec[] args) 
        {
            Type = type;
            CtorMethod = ctorMethod;
            PreDispatchMethod = preDispatchMethod;
            DispatchMethod = dispatchMethod;
            ReturnField = returnField;
            ArgsField = argsField;
            Args = args;
            RequiresInstanciation = Type.HasGenericParameters;
            ReturnsValue = ReturnField != null;
        }

        public CallInfo Instanciate(IEnumerable<TypeReference> genArgs) {            
            var tCallRef = Type.MakeGenericInstanceType(genArgs.ToArray());
            var tCallBaseRef = tCallRef.Resolve().BaseType;

            var mod = Type.Module;

            return new CallInfo(tCallRef,
                                    tCallRef.ReferenceMethod(m => m.IsConstructor),
                                    tCallBaseRef.ReferenceMethod(PreDispatchMethod.Name),
                                    tCallBaseRef.ReferenceMethod(DispatchMethod.Name),
                                    ReturnsValue 
                                        ? mod.ImportReference(tCallBaseRef.ReferenceField(ReturnField.Name)) 
                                        : null,
                                    mod.ImportReference(tCallBaseRef.ReferenceField(ArgsField.Name)),
                                    Args
                                    );
        }

    }



}
