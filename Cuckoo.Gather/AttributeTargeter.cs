using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cuckoo.Gather
{
    internal class AttributeTargeter : IRoostPicker
    {
        public IEnumerable<RoostSpec> PickRoosts(Assembly assembly) 
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
                                    .Where(a => typeof(ICuckooProvider).IsAssignableFrom(a.Constructor.DeclaringType))
                                    .ToArray()
                            
                        })
                        .Where(t => t.Atts.Any());
            

            return tups.SelectMany(
                    t => t.Atts.Select(
                        att => {
                            var ctorArgs = att.ConstructorArguments
                                                .Select(a => a.Value);

                            var namedArgs = att.NamedArguments
                                                .Select(a => new KeyValuePair<string, object>(
                                                                                a.MemberInfo.Name, 
                                                                                a.TypedValue.Value ));
                            
                            return new RoostSpec(
                                            t.Method,
                                            att.Constructor,
                                            ctorArgs.ToArray(),
                                            namedArgs.ToArray() );
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
