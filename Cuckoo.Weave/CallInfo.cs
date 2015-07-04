using Cuckoo.Weave.Cecil;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cuckoo.Weave
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
        public MethodReference PrepareMethod { get; private set; }
        public MethodReference InvokeMethod { get; private set; }
        public FieldReference RoostField { get; private set; }
        public FieldReference InstanceField { get; private set; }
        public FieldReference ReturnField { get; private set; }
        public FieldReference ArgsField { get; private set; }
        public FieldReference ArgFlagsField { get; private set; }
        public CallWeaver.ArgSpec[] Args { get; private set; }
        public bool RequiresInstanciation { get; private set; }
        public bool ReturnsValue { get; private set; }


        public CallInfo(
                    TypeReference type,
                    MethodReference ctorMethod,
                    MethodReference prepareMethod,
                    MethodReference invokeMethod,
                    FieldReference roostField,
                    FieldReference instanceField,
                    FieldReference returnField,
                    FieldReference argsField,
                    FieldReference argFlagsField,
                    CallWeaver.ArgSpec[] args ) 
        {
            Type = type;
            CtorMethod = ctorMethod;
            PrepareMethod = prepareMethod;
            InvokeMethod = invokeMethod;
            RoostField = roostField;
            InstanceField = instanceField;
            ReturnField = returnField;
            ArgsField = argsField;
            ArgFlagsField = argFlagsField;
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

            var tCallBaseRef = tCallRef.GetBaseType();    //THIS IS WHAT DOES IT!!!! BUT ONLY FOR METHOD WITH GENERIC RETURN TYPE!!!

            var mod = Type.Module;

            return new CallInfo(tCallRef,
                                    tCallRef.ReferenceMethod(m => m.IsConstructor && !m.IsStatic),
                                    tCallBaseRef.ReferenceMethod(PrepareMethod.Name),
                                    tCallBaseRef.ReferenceMethod(InvokeMethod.Name),
                                    tCallRef.ReferenceField(RoostField.Name),
                                    tCallBaseRef.ReferenceField(InstanceField.Name),
                                    ReturnsValue
                                        ? tCallBaseRef.ReferenceField(ReturnField.Name)
                                        : null,
                                    tCallBaseRef.ReferenceField(ArgsField.Name),
                                    tCallBaseRef.ReferenceField(ArgFlagsField.Name),
                                    Args
                                    );
        }

    }



}
