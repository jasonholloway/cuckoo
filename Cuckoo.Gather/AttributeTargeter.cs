using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cuckoo.Gather
{
    internal class AttributeTargeter : IRoostTargeter
    {
        public IEnumerable<RoostTarget> TargetRoosts(Assembly assembly) 
        {
            var allTypes = GetAllTypes(assembly);

            var bindingFlags = BindingFlags.Instance
                                | BindingFlags.Static
                                | BindingFlags.Public
                                | BindingFlags.NonPublic;

            var allMethods = allTypes.SelectMany(t => t.GetMethods(bindingFlags)
                                                        .Where(m => m.DeclaringType == t)
                                                        .Cast<MethodBase>())
                                        .Concat(allTypes
                                                .SelectMany(t => t.GetConstructors(bindingFlags)));

            var tups = allMethods
                        .Select(m => new {
                            Method = m,
                            Atts = m.GetCustomAttributesData()
                                    .Where(a => typeof(ICuckooHatcher).IsAssignableFrom(a.Constructor.DeclaringType))
                                    .ToArray()
                            
                        })
                        .Where(t => t.Atts.Any());
            

            return tups.SelectMany(
                    t => t.Atts.Select(
                        att => {
                            var ctorArgs = att.ConstructorArguments
                                                .Select(a => {
                                                    if(a.Value is IEnumerable<CustomAttributeTypedArgument>) 
                                                    {
                                                        var vals = ((IEnumerable<CustomAttributeTypedArgument>)a.Value).ToArray();

                                                        var tEl = a.ArgumentType.GetElementType(); 

                                                        var r = Array.CreateInstance(tEl, vals.Length);

                                                        for(int i = 0; i < vals.Length; i++) {
                                                            var val = vals[i];
                                                            r.SetValue(val.Value, i);
                                                        }

                                                        return r;
                                                    }

                                                    return a.Value;
                                                });

                            var dNamedArgs = att.NamedArguments
                                                .ToDictionary(
                                                    a => a.MemberInfo.Name,
                                                    a => a.TypedValue.Value
                                                );

                            return new RoostTarget(
                                            t.Method,
                                            att.Constructor,
                                            ctorArgs.ToArray(),
                                            dNamedArgs);
                        })).ToArray();
        }



        IEnumerable<Type> GetAllTypes(Assembly assembly) {
            return assembly.GetTypes()
                            .SelectMany(t => GetAllTypes(t));
        }

        IEnumerable<Type> GetAllTypes(Type type) {
            yield return type;

            foreach(var nestedType in type.GetNestedTypes()) {
                foreach(var t in GetAllTypes(nestedType)) {
                    yield return t;
                }
            }
        }

    }




}
