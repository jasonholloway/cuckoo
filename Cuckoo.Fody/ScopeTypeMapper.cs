using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    class ScopeTypeMapper
    {
        ModuleDefinition _module;
        IGenericParameterProvider _genProv;
        Dictionary<TypeReference, TypeReference> _dMap;

        public ScopeTypeMapper(IGenericParameterProvider genProv) {
            _module = genProv.Module;
            _genProv = genProv;
            _dMap = new Dictionary<TypeReference, TypeReference>(TypeRefEqualityComparer.Default);
        }

        public TypeReference Map(TypeReference sourceType) {
            if(sourceType is TypeSpecification) {
                sourceType = Map(sourceType.GetElementType());
            }

            TypeReference type = null;

            if(!_dMap.TryGetValue(sourceType, out type)) {
                if(sourceType.IsGenericParameter) {
                    var oldGen = (GenericParameter)sourceType;
                    var newGen = new GenericParameter(oldGen.Name, _genProv);

                    newGen.HasDefaultConstructorConstraint = oldGen.HasDefaultConstructorConstraint;
                    newGen.HasNotNullableValueTypeConstraint = oldGen.HasNotNullableValueTypeConstraint;
                    newGen.HasReferenceTypeConstraint = oldGen.HasReferenceTypeConstraint;

                    foreach(var typeConstraint in oldGen.Constraints) {
                        newGen.Constraints.Add(this.Map(typeConstraint));
                    }

                    _genProv.GenericParameters.Add(newGen);

                    type = newGen;
                }
                else {
                    type = _module.ImportReference(sourceType);
                }

                _dMap[sourceType] = type;
            }

            if(sourceType is ByReferenceType) {
                type = new ByReferenceType(type);
            }

            if(sourceType is ArrayType) {
                type = new ArrayType(type, ((ArrayType)sourceType).Rank);
            }

            return type;
        }
        


        class TypeRefEqualityComparer : IEqualityComparer<TypeReference>
        {
            public static readonly TypeRefEqualityComparer Default = new TypeRefEqualityComparer();

            public bool Equals(TypeReference x, TypeReference y) {

                if(x.FullName != y.FullName) {
                    return false;
                }
                
                if(x is GenericParameter && y is GenericParameter) {
                    var genX = x as GenericParameter;
                    var genY = y as GenericParameter;

                    if(genX.DeclaringType != null && genY.DeclaringType != null) {
                        return Equals(genX.DeclaringType, genY.DeclaringType);
                    }

                    if(genX.DeclaringMethod != null && genY.DeclaringMethod != null) {
                        return genX.DeclaringMethod.FullName == genY.DeclaringMethod.FullName
                                && genX.Module.Assembly.Name == genY.Module.Assembly.Name;
                    }

                    return false;
                }
                
                return x.Module.Assembly.Name == y.Module.Assembly.Name;
            }

            public int GetHashCode(TypeReference obj) {
                return obj.MetadataToken.GetHashCode();
            }
        }

    }
}
 