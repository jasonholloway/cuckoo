using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;
using Sequences;
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

        public static Type FullySpecify(Type type) {
            var decType = type.DeclaringType;

            if(decType != null && decType.ContainsGenericParameters) {
                decType = FullySpecify(decType);
                type = decType.GetNestedType(type.Name);
            }

            if(type.IsGenericTypeDefinition) {
                var genArgs = Sequence.Fill(typeof(string), type.GetGenericArguments().Length)
                                        .ToArray();

                type = type.MakeGenericType(genArgs);
            }
            
            return type;
        }

        public static FieldInfo FullySpecify(FieldInfo field) {
            var decType = FullySpecify(field.DeclaringType);

            field = decType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                            .First(f => f.Name == field.Name);

            return field;
        }

        public static MethodInfo FullySpecify(MethodInfo method) 
        {
            var decType = FullySpecify(method.DeclaringType);
            method = decType.GetMethods()
                                .First(m => m.Name == method.Name
                                            && m.GetParameters().Select(p => p.ParameterType)
                                                    .SequenceEqual(method.GetParameters().Select(p => p.ParameterType)));

            if(method.IsGenericMethodDefinition) {
                var genArgs = Sequence.Fill(typeof(string), method.GetGenericArguments().Length)
                                        .ToArray();

                method = method.MakeGenericMethod(genArgs);
            }

            return method;
        }



        public static object Test(MethodInfo method) 
        {
            method = FullySpecify(method);

            object instance = null;

            if(!method.IsStatic) {
                instance = Activator.CreateInstance(method.DeclaringType);
            }

            var args = method.GetParameters()
                        .Select(p => {
                            switch(p.ParameterType.Name) {
                                case "String":
                                    return GetRandom.String(8);

                                case "Int32":
                                    return (object)GetRandom.Int();

                                case "Single":
                                    return default(Single);
                            }

                            throw new InvalidOperationException();
                        })
                        .ToArray();
                        
            object returnVal = method.Invoke(instance, args);

            return returnVal;
        }

    }
}
