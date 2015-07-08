using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo.Gather.Monikers
{
    public partial class MonikerGenerator
    {
        Dictionary<MethodBase, IMethodMoniker> _dMethods = new Dictionary<MethodBase, IMethodMoniker>();

        public IMethodMoniker Method(MethodBase method) 
        {
            IMethodMoniker moniker = null;

            if(!_dMethods.TryGetValue(method, out moniker)) {
                moniker = CreateMethodMoniker(method);
                _dMethods[method] = moniker;
            }

            return moniker;
        }


        IMethodMoniker CreateMethodMoniker(MethodBase method) 
        {
            if(method.IsGenericMethod && !method.IsGenericMethodDefinition) {
                var info = (MethodInfo)method;

                return new GenMethodSpec(
                                this.Method(info.GetGenericMethodDefinition()),
                                info.GetGenericArguments()
                                        .Select(a => this.Type(a))
                                        .ToArray()
                                );
            }

            return new MethodDef(
                            this.Type(method.DeclaringType),
                            method.Name,
                            method.GetParameters()
                                    .Select(p => this.Type(p.ParameterType))
                                    .ToArray(),
                            method.MetadataToken
                            );
        }

    }
}
