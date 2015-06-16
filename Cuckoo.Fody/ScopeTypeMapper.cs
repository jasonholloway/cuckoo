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

        public TypeReference Map(TypeReference foreignType) {

            //NEED TO STRIP OF ADDITIONAL SPECS N MAP ONLY ELEMENT TYPE
            //THEN RETURN MAPPED TYPE APPROPRIATELY ACCOUTRED WITH SPECS
            //...

            TypeReference localType = null;

            if(!_dMap.TryGetValue(foreignType, out localType)) {
                if(foreignType.IsGenericParameter) {
                    var oldGen = (GenericParameter)foreignType;
                    var newGen = new GenericParameter(oldGen.Name, _genProv);

                    newGen.HasDefaultConstructorConstraint = oldGen.HasDefaultConstructorConstraint;
                    newGen.HasNotNullableValueTypeConstraint = oldGen.HasNotNullableValueTypeConstraint;
                    newGen.HasReferenceTypeConstraint = oldGen.HasReferenceTypeConstraint;

                    foreach(var typeConstraint in oldGen.Constraints) {
                        newGen.Constraints.Add(this.Map(typeConstraint));
                    }

                    _genProv.GenericParameters.Add(newGen);

                    localType = newGen;
                }
                else {
                    localType = _module.ImportReference(foreignType);
                }

                _dMap[foreignType] = localType;
            }

            return localType;
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
 