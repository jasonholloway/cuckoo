using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sequences;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Test.Infrastructure
{
    public class MethodTester
    {
        /*
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
        
        static bool MethodsAreEqual(MethodInfo x, MethodInfo y) {
            return x.Name == y.Name
                    && x.DeclaringType.FullName == y.DeclaringType.FullName
                    && x.ReturnType == y.ReturnType
                    && x.GetParameters().Select(p => p.ParameterType)
                            .SequenceEqual(y.GetParameters().Select(p => p.ParameterType));
        }
        */
        
        class Mapper
        {
            Dictionary<Type, Type> _dTypes;
            Dictionary<MethodInfo, MethodInfo> _dMethods;
            Dictionary<MemberInfo, MemberInfo> _dMembers;

            public Mapper(Module module) {
                _dTypes = module.GetTypes()
                                    .ToDictionary(t => t, TypeComparer.Instance);
                
                _dMethods = _dTypes.Values.SelectMany(t => t.GetMethods())
                                            .Distinct(MethodComparer.Instance)
                                            .ToDictionary(m => m, MethodComparer.Instance);

                _dMembers = _dTypes.Values.SelectMany(t => t.GetMembers())
                                            .Distinct(MemberComparer.Instance)
                                            .ToDictionary(m => m, MemberComparer.Instance);
            }

            internal Type Map(Type type) {
                Type foundType;

                if(_dTypes.TryGetValue(type, out foundType)) {
                    return foundType;
                }

                return type;
            }
            
            internal MethodInfo Map(MethodInfo method) {
                MethodInfo foundMethod;

                if(_dMethods.TryGetValue(method, out foundMethod)) {
                    return foundMethod;
                }

                return method;
            }

            internal MemberInfo Map(MemberInfo member) {
                MemberInfo foundMember;

                if(_dMembers.TryGetValue(member, out foundMember)) {
                    return foundMember;
                }

                return member;
            }

            class TypeComparer : IEqualityComparer<Type>
            {
                public static readonly TypeComparer Instance = new TypeComparer();

                public bool Equals(Type x, Type y) {
                    return x.FullName == y.FullName;
                }

                public int GetHashCode(Type obj) {
                    return obj.FullName.GetHashCode();
                }
            }

            class MemberComparer : IEqualityComparer<MemberInfo>
            {
                public static readonly MemberComparer Instance = new MemberComparer();

                public bool Equals(MemberInfo x, MemberInfo y) {
                    if(x is MethodInfo && y is MethodInfo) {
                        return MethodComparer.Instance.Equals((MethodInfo)x, (MethodInfo)y);
                    }

                    return TypeComparer.Instance.Equals(x.DeclaringType, y.DeclaringType)
                            && x.Name == y.Name;
                }

                public int GetHashCode(MemberInfo obj) {
                    return (obj.DeclaringType.FullName + obj.Name).GetHashCode();
                }
            }

            class MethodComparer : IEqualityComparer<MethodInfo>
            {
                public static readonly MethodComparer Instance = new MethodComparer();

                public bool Equals(MethodInfo x, MethodInfo y) {
                    return x.Name == y.Name
                            && TypeComparer.Instance.Equals(x.DeclaringType, y.DeclaringType)
                            && TypeComparer.Instance.Equals(x.ReturnType, y.ReturnType)
                            && x.GetParameters().SequenceEqual(y.GetParameters(), ParameterComparer.Instance);
                }

                public int GetHashCode(MethodInfo obj) {
                    return (obj.Name + obj.DeclaringType.FullName + obj.ReturnType.FullName).GetHashCode();
                }
            }

            class ParameterComparer : IEqualityComparer<ParameterInfo>
            {
                public static readonly ParameterComparer Instance = new ParameterComparer();

                public bool Equals(ParameterInfo x, ParameterInfo y) {
                    return TypeComparer.Instance.Equals(x.ParameterType, y.ParameterType)
                            && x.IsOut == y.IsOut
                            && x.Position == y.Position;
                }

                public int GetHashCode(ParameterInfo obj) {
                    return MemberComparer.Instance.GetHashCode(obj.Member) ^ TypeComparer.Instance.GetHashCode(obj.ParameterType);
                }
            }

        }


        class Replacer : ExpressionVisitor
        {
            Mapper _mapper;
            Dictionary<ParameterExpression, ParameterExpression> _dParamExps;

            public Replacer(Mapper mapper) {
                _mapper = mapper;
                _dParamExps = new Dictionary<ParameterExpression, ParameterExpression>();
            }

            protected override Expression VisitLambda<TDelegate>(Expression<TDelegate> node) {
                var delType = typeof(TDelegate);
                var funcType = delType.GetGenericTypeDefinition();
                var genTypes = delType.GetGenericArguments();

                var newDelType = funcType.MakeGenericType(
                                            genTypes.Select(t => _mapper.Map(t)).ToArray());
                
                return Expression.Lambda(
                                    newDelType,
                                    Visit(node.Body),
                                    node.Parameters.Select(p => Visit(p) as ParameterExpression).ToArray()
                                    );
            }
            
            protected override Expression VisitParameter(ParameterExpression node) {
                ParameterExpression newNode;

                if(!_dParamExps.TryGetValue(node, out newNode)) {
                    newNode = Expression.Parameter(
                                            _mapper.Map(node.Type),
                                            node.Name
                                            );

                    _dParamExps[node] = newNode;
                }

                return newNode;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node) {
                var instance = Visit(node.Object);
                var method = _mapper.Map(node.Method);
                var args = node.Arguments.Select(a => Visit(a)).ToArray();

                return Expression.Call(
                                    instance,
                                    method,
                                    args
                                    );
            }

            protected override Expression VisitMember(MemberExpression node) {
                return Expression.MakeMemberAccess(
                                    Visit(node.Expression),
                                    _mapper.Map(node.Member)
                                    );
            }

            protected override Expression VisitNew(NewExpression node) {
                return Expression.New(
                                    (ConstructorInfo)_mapper.Map(node.Constructor),
                                    node.Arguments.Select(a => Visit(a))
                                                    .ToArray()
                                    );
            }

        }




        Mapper _mapper;

        public MethodTester(Module newModule) {
            _mapper = new Mapper(newModule);
        }





        //public TRet Run<TBase,TRet>(Expression<Func<TBase,TRet>> ex) 
        //{           
        //    var replacer = new Replacer(_mapper);

        //    var newEx = (LambdaExpression)replacer.Visit(ex);                        
        //    var newFn = newEx.Compile();

        //    var baseType = _mapper.Map(typeof(TBase));
        //    object baseObj = Activator.CreateInstance(baseType);

        //    var result = newFn.DynamicInvoke(baseObj);

        //    return (TRet)result;
        //}



        public IClassMethodTester<TClass> WithClass<TClass>() {
            return new ClassMethodTester<TClass>(_mapper);
        }




        public interface IClassMethodTester<TClass>
        {
            TResult Run<TResult>(Expression<Func<TClass,TResult>> exFn);
        }

        class ClassMethodTester<TClass>
            : IClassMethodTester<TClass>
        {
            Mapper _mapper;

            public ClassMethodTester(Mapper mapper) {
                _mapper = mapper;
            }

            public TResult Run<TResult>(Expression<Func<TClass, TResult>> exFn) {
                var replacer = new Replacer(_mapper);

                var newEx = (LambdaExpression)replacer.Visit(exFn);
                var newFn = newEx.Compile();

                var baseType = _mapper.Map(typeof(TClass));
                object baseObj = Activator.CreateInstance(baseType);

                var result = newFn.DynamicInvoke(baseObj);

                return (TResult)result;
            }
        }



        /*
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
        */
    }
}
