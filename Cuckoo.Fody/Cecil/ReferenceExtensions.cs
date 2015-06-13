// Courtesy of Gábor Kozár

using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;

namespace Cuckoo.Fody.Cecil
{
    public static class ReferenceExtensions
    {
        private static MethodReference _CloneMethodWithDeclaringType(MethodDefinition methodDef, TypeReference declaringTypeRef) {
            if(!declaringTypeRef.IsGenericInstance || methodDef == null) {
                return methodDef;
            }

            var methodRef = new MethodReference(methodDef.Name, methodDef.ReturnType, declaringTypeRef) {
                CallingConvention = methodDef.CallingConvention,
                HasThis = methodDef.HasThis,
                ExplicitThis = methodDef.ExplicitThis
            };

            foreach(ParameterDefinition paramDef in methodDef.Parameters) {
                methodRef.Parameters.Add(new ParameterDefinition(paramDef.Name, paramDef.Attributes, paramDef.ParameterType));
            }

            foreach(GenericParameter genParamDef in methodDef.GenericParameters) {
                methodRef.GenericParameters.Add(new GenericParameter(genParamDef.Name, methodRef));
            }

            return methodRef;
        }

        public static MethodReference ReferenceMethod(this TypeReference typeRef, Func<MethodDefinition, bool> methodSelector) {
            return _CloneMethodWithDeclaringType(typeRef.Resolve().Methods.FirstOrDefault(methodSelector), typeRef);
        }

        public static MethodReference ReferenceMethod(this TypeReference typeRef, string methodName) {
            return ReferenceMethod(typeRef, m => m.Name == methodName);
        }

        public static MethodReference ReferenceMethod(this TypeReference typeRef, string methodName, int paramCount) {
            return ReferenceMethod(typeRef, m => m.Name == methodName && m.Parameters.Count == paramCount);
        }

        public static MethodReference ReferenceMethod(this TypeReference typeRef, string methodName, params TypeReference[] parameterTypes) {
            return ReferenceMethod(typeRef, m => m.Parameters.Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        }

        public static FieldReference ReferenceField(this TypeReference typeRef, Func<FieldDefinition, bool> fieldSelector) {
            FieldDefinition fieldDef = typeRef.Resolve().Fields.FirstOrDefault(fieldSelector);
            if(!typeRef.IsGenericInstance || fieldDef == null) {
                return fieldDef;
            }

            return new FieldReference(fieldDef.Name, fieldDef.FieldType, typeRef);
        }

        public static FieldReference ReferenceField(this TypeReference typeRef, string fieldName) {
            return ReferenceField(typeRef, f => f.Name == fieldName);
        }

        public static MethodReference ReferencePropertyGetter(this TypeReference typeRef, Func<PropertyDefinition, bool> propertySelector) {
            PropertyDefinition propDef = typeRef.Resolve().Properties.FirstOrDefault(propertySelector);
            if(propDef == null || propDef.GetMethod == null) {
                return null;
            }

            return _CloneMethodWithDeclaringType(propDef.GetMethod, typeRef);
        }

        public static MethodReference ReferencePropertyGetter(this TypeReference typeRef, string propertyName) {
            return ReferencePropertyGetter(typeRef, p => p.Name == propertyName);
        }

        public static MethodReference ReferencePropertySetter(this TypeReference typeRef, Func<PropertyDefinition, bool> propertySelector) {
            PropertyDefinition propDef = typeRef.Resolve().Properties.FirstOrDefault(propertySelector);
            if(propDef == null || propDef.SetMethod == null) {
                return null;
            }

            return _CloneMethodWithDeclaringType(propDef.SetMethod, typeRef);
        }

        public static MethodReference ReferencePropertySetter(this TypeReference typeRef, string propertyName) {
            return ReferencePropertySetter(typeRef, p => p.Name == propertyName);
        }
    }
}

