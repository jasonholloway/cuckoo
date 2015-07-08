﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Gather.Monikers
{
    public static class TypeMoniker
    {
        public static ITypeMoniker Derive(Type type) {
            if(type.IsValueType && type.IsByRef) {
                return new ByRefTypeSpec(
                                Derive(type.GetElementType())
                                );
            }

            if(type.IsArray) {
                return new ArrayTypeSpec(
                                Derive(type.GetElementType()),
                                type.GetArrayRank()
                                );
            }
            else if(type.IsGenericType && !type.IsGenericTypeDefinition) {
                return new GenTypeSpec(
                                Derive(type.GetGenericTypeDefinition()),
                                type.GetGenericArguments()
                                        .Select(a => Derive(a))
                                        .ToArray()
                                );
            }


            if(type.IsGenericParameter) {
                return new GenParamSpec(
                                //...                
                                );
            }
            

            if(type.IsNested) {
                return new NestedTypeDef(
                                Derive(type.DeclaringType),
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



    public interface ITypeMoniker
    {
        string AssemblyName { get; }
        string Name { get; }
        string FullName { get; }
        string AssemblyQualifiedName { get; }
    }


    [Serializable]
    public class TypeDef : ITypeMoniker
    {
        public string AssemblyName { get; private set; }
        public string Namespace { get; private set; }
        public string Name { get; private set; }

        public TypeDef(string asmName, string @namespace, string name) {
            AssemblyName = asmName;
            Namespace = @namespace;
            Name = name;
        }

        public string FullName {
            get { return string.Format("{0}.{1}", Namespace, Name); }
        }

        public string AssemblyQualifiedName {
            get { return string.Format("{0}, {1}", FullName, AssemblyName); }
        }
    }

    [Serializable]
    public class NestedTypeDef : ITypeMoniker
    {
        public ITypeMoniker DeclaringType { get; private set; }
        public string Name { get; private set; }

        public NestedTypeDef(ITypeMoniker decType, string name) {
            DeclaringType = decType;
            Name = name;
        }

        public string AssemblyName {
            get { return DeclaringType.AssemblyName; }
        }

        public string FullName {
            get { return string.Format("{0}+{1}", DeclaringType.FullName, Name); }
        }

        public string AssemblyQualifiedName {
            get { return string.Format("{0}, {1}", FullName, AssemblyName); }
        }
    }



    [Serializable]
    public class ArrayTypeSpec : ITypeMoniker
    {
        public ITypeMoniker ElementType { get; private set; }
        public int Rank { get; private set; }

        public ArrayTypeSpec(ITypeMoniker elemType, int rank) {
            ElementType = elemType;
            Rank = rank;
        }

        public string AssemblyName {
            get { return ElementType.AssemblyName; }
        }

        public string Name {
            get { return string.Format("{0}[]", ElementType.Name); }
        }

        public string FullName {
            get { return string.Format("{0}[]", ElementType.FullName); }
        }

        public string AssemblyQualifiedName {
            get { return string.Format("{0}, {1}", FullName, AssemblyName); }
        }
    }

    [Serializable]
    public class ByRefTypeSpec : ITypeMoniker
    {
        public ITypeMoniker ElementType { get; private set; }

        public ByRefTypeSpec(ITypeMoniker elemType) {
            ElementType = elemType;
        }

        public string AssemblyName {
            get { return ElementType.AssemblyName; }
        }

        public string Name {
            get { return string.Format("{0}&", ElementType.Name); }
        }

        public string FullName {
            get { return string.Format("{0}&", ElementType.FullName); }
        }

        public string AssemblyQualifiedName {
            get { return string.Format("{0}, {1}", FullName, AssemblyName); }
        }
    }


    [Serializable]
    public class GenTypeSpec : ITypeMoniker
    {
        public ITypeMoniker ElementType { get; private set; }
        public ITypeMoniker[] GenArgs { get; private set; }
        

        public GenTypeSpec(ITypeMoniker elemType, ITypeMoniker[] genArgs) {
            ElementType = elemType;
            GenArgs = genArgs;
        }

        public string AssemblyName {
            get { return ElementType.AssemblyName; }
        }
        
        public string Name {
            get {
                var sb = new StringBuilder();

                sb.Append(ElementType.Name);
                sb.Append("[");

                bool isSuccessive = false;

                foreach(var genArg in GenArgs) {
                    if(isSuccessive) {
                        sb.Append(",");
                    }

                    sb.Append("[");
                    sb.Append(genArg.Name);
                    sb.Append("]");

                    isSuccessive = true;
                }

                sb.Append("]");

                return sb.ToString();
            }
        }

        public string FullName {
            get {
                var sb = new StringBuilder();

                sb.Append(ElementType.FullName);
                sb.Append("[");

                bool isSuccessive = false;

                foreach(var genArg in GenArgs) {
                    if(isSuccessive) {
                        sb.Append(",");
                    }

                    sb.Append("[");
                    sb.Append(genArg.FullName);
                    sb.Append("]");

                    isSuccessive = true;
                }

                sb.Append("]");

                return sb.ToString();
            }
        }

        public string AssemblyQualifiedName {
            get { return string.Format("{0}, {1}", FullName, AssemblyName); }
        }
    }

    [Serializable]
    public class GenParamSpec : ITypeMoniker
    {

        public string AssemblyName {
            get { throw new NotImplementedException(); }
        }

        public string Name {
            get { throw new NotImplementedException(); }
        }

        public string FullName {
            get { throw new NotImplementedException(); }
        }

        public string AssemblyQualifiedName {
            get { throw new NotImplementedException(); }
        }
    }


}
