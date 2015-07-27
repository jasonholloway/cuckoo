using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Gather.Monikers
{
    public partial class MonikerGenerator
    {
        Dictionary<Type, ITypeMoniker> _dTypes = new Dictionary<Type, ITypeMoniker>();

        public ITypeMoniker Type(Type type) 
        {
            ITypeMoniker moniker = null;

            if(!_dTypes.TryGetValue(type, out moniker)) {
                moniker = CreateTypeMoniker(type);
                _dTypes[type] = moniker;
            }

            return moniker;
        }


        ITypeMoniker CreateTypeMoniker(Type type) 
        {            
            if(type.IsValueType && type.IsByRef) {
                return new ByRefTypeSpec(
                                this.Type(type.GetElementType())
                                );
            }

            if(type.IsArray) {
                return new ArrayTypeSpec(
                                this.Type(type.GetElementType()),
                                type.GetArrayRank()
                                );
            }
            else if(type.IsGenericType && !type.IsGenericTypeDefinition) {
                return new GenTypeSpec(
                                this.Type(type.GetGenericTypeDefinition()),
                                type.GetGenericArguments()
                                        .Select(a => this.Type(a))
                                        .ToArray()
                                );
            }


            if(type.IsGenericParameter) {
                if(type.DeclaringMethod != null) {
                    return new MethodGenParam(
                                    type.Name,
                                    type.GenericParameterPosition
                                    );
                }
                else {
                    return new TypeGenParam(
                                    type.Name,
                                    type.GenericParameterPosition
                                    );
                }
            }
            

            if(type.IsNested) {
                return new NestedTypeDef(
                                this.Type(type.DeclaringType),
                                type.Name
                                );
            }

            return new TypeDef(
                            type.Assembly.FullName,
                            type.Namespace,
                            type.Name
                            );
        }

    }
}
