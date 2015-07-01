// Courtesy of Gábor Kozár plus additions

using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;

namespace Cuckoo.Fody.Cecil
{
    public static class ReferenceExtensions
    {

        public static FieldReference CloneWithNewDeclaringType(this FieldDefinition @this, TypeReference declaringTypeRef) {
            if(!declaringTypeRef.IsGenericInstance || @this == null) {
                return @this;
            }

            var fieldRef = new FieldReference(@this.Name, @this.FieldType, declaringTypeRef) {
                //...
            };

            return fieldRef;
        }
        

        public static MethodReference CloneWithNewDeclaringType(this MethodDefinition @this, TypeReference declaringTypeRef) {
            if(!declaringTypeRef.IsGenericInstance || @this == null) {
                return @this;
            }

            var methodRef = new MethodReference(@this.Name, @this.ReturnType, declaringTypeRef) {
                CallingConvention = @this.CallingConvention,
                HasThis = @this.HasThis,
                ExplicitThis = @this.ExplicitThis
            };

            foreach(ParameterDefinition paramDef in @this.Parameters) {
                methodRef.Parameters.Add(new ParameterDefinition(
                                                            paramDef.Name, 
                                                            paramDef.Attributes, 
                                                            declaringTypeRef.Module.Import(paramDef.ParameterType, methodRef)
                                                            ));
            }

            foreach(GenericParameter genParamDef in @this.GenericParameters) {
                methodRef.GenericParameters.Add(new GenericParameter(genParamDef.Name, methodRef));
            }

            return methodRef;
        }

        public static MethodReference ReferenceMethod(this TypeReference typeRef, Func<MethodDefinition, bool> methodSelector) {
            return typeRef.Resolve().Methods
                                        .FirstOrDefault(methodSelector)
                                        .CloneWithNewDeclaringType(typeRef);
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

            if(fieldDef == null) return null;

            var fieldType = fieldDef.FieldType;

            if(fieldType.IsGenericParameter) {
                //var typeInst = (GenericInstanceType)typeRef;

                //var tups = typeInst.ElementType.GenericParameters
                //                .Zip(typeInst.GenericArguments,
                //                                    (p, a) => new {
                //                                        Param = p,
                //                                        Arg = a
                //                                    }).ToArray();

                //var matchedTup = tups.First(t => t.Param.Name == fieldType.Name
                //                                    && t.Param.DeclaringType.FullName == fieldType.DeclaringType.FullName);

                //fieldType = matchedTup.Param; //.Arg;
            }
            else {
                fieldType = typeRef.Module.Import(fieldType);
            }

            return new FieldReference(fieldDef.Name, fieldType, typeRef);
        }

        public static FieldReference ReferenceField(this TypeReference typeRef, string fieldName) {
            return ReferenceField(typeRef, f => f.Name == fieldName);
        }

        public static MethodReference ReferencePropertyGetter(this TypeReference typeRef, Func<PropertyDefinition, bool> propertySelector) {
            PropertyDefinition propDef = typeRef.Resolve().Properties.FirstOrDefault(propertySelector);
            if(propDef == null || propDef.GetMethod == null) {
                return null;
            }

            return propDef.GetMethod.CloneWithNewDeclaringType(typeRef);
        }

        public static MethodReference ReferencePropertyGetter(this TypeReference typeRef, string propertyName) {
            return ReferencePropertyGetter(typeRef, p => p.Name == propertyName);
        }

        public static MethodReference ReferencePropertySetter(this TypeReference typeRef, Func<PropertyDefinition, bool> propertySelector) {
            PropertyDefinition propDef = typeRef.Resolve().Properties.FirstOrDefault(propertySelector);
            if(propDef == null || propDef.SetMethod == null) {
                return null;
            }

            return propDef.SetMethod.CloneWithNewDeclaringType(typeRef);
        }

        public static MethodReference ReferencePropertySetter(this TypeReference typeRef, string propertyName) {
            return ReferencePropertySetter(typeRef, p => p.Name == propertyName);
        }
    }
}

