using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Test.Infrastructure
{
    public class MethodTester
    {
        public static object Test(MethodInfo method) {
            object instance = null;

            if(!method.IsStatic) {
                instance = Activator.CreateInstance(method.DeclaringType);
            }

            var args = method.GetParameters()
                        .Select(p => {
                            switch(p.ParameterType.Name.ToLower()) {
                                case "string":
                                    return GetRandom.String(8);

                                case "int":
                                    return (object)GetRandom.Int();
                            }

                            return null;
                        })
                        .ToArray();

            object returnVal = method.Invoke(instance, args);

            return returnVal;
        }

    }
}
