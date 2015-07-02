using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cuckoo.Gather
{
    internal class AttributeRoostTargeter : IRoostTargeter
    {
        public IEnumerable<TargetRoost> GetTargets(Assembly assembly) 
        {
            var allTypes = GetAllTypes(assembly);

            var bindingFlags = BindingFlags.Instance
                                | BindingFlags.Static
                                | BindingFlags.Public
                                | BindingFlags.NonPublic;

            var allMethods = allTypes.SelectMany(t => t.GetMethods(bindingFlags).Cast<MethodBase>())
                                        .Concat(allTypes.SelectMany(t => t.GetConstructors(bindingFlags)));

            var tups = allMethods
                        .Select(m => new {
                            Method = m,
                            Atts = m.GetCustomAttributesData()
                                    .Where(a => typeof(ICuckooProvider).IsAssignableFrom(a.Constructor.DeclaringType))
                                    .ToArray()
                            
                        })
                        .Where(t => t.Atts.Any());

            //But what about ctor data?


            //as currently envisioned, CuckooProviders are specified solely by type, ready for them to be
            //constructed at run time.

            //Specs need to store serializable ctor args it seems

            

            //And our targets need something better than method name, as there are overloads...
            //maybe we should fall back on using the token


            return tups
                    .SelectMany(t => t.Atts.Select(a => {
                        return new TargetRoost(
                                        t.Method, 
                                        a.Constructor.DeclaringType );
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
