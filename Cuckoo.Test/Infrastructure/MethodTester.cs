﻿using FizzWare.NBuilder;
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
   

        class Mapper
        {
            Dictionary<Type, Type> _dTypes;
            Dictionary<MethodBase, MethodBase> _dMethods;
            Dictionary<MemberInfo, MemberInfo> _dMembers;

            public Mapper(Module module) {
                _dTypes = module.GetTypes()
                                    .OrderBy(t => t.FullName)
                                    .ToDictionary(t => t, TypeComparer.Instance);
                
                _dMethods = _dTypes.Values.SelectMany(t => t.GetMethods().Cast<MethodBase>()
                                                                .Concat(t.GetConstructors().Cast<MethodBase>()))
                                            .Distinct(MethodComparer.Instance)
                                            .OrderBy(m => m.DeclaringType.FullName + "." + m.Name)
                                            .ToDictionary(m => m, MethodComparer.Instance);

                _dMembers = _dTypes.Values.SelectMany(t => t.GetMembers())
                                            .Distinct(MemberComparer.Instance)
                                            .OrderBy(m => m.DeclaringType.FullName + "." + m.Name)
                                            .ToDictionary(m => m, MemberComparer.Instance);
            }

            internal Type Map(Type type) {
                Type[] genArgs = null;

                if(type.IsGenericType) {
                    genArgs = type.GetGenericArguments()
                                    .Select(t => Map(t))
                                    .ToArray();

                    type = type.GetGenericTypeDefinition();
                }

                Type foundType;

                if(_dTypes.TryGetValue(type, out foundType)) {
                    type = foundType;
                }

                if(genArgs != null) {
                    type = type.MakeGenericType(genArgs);
                }

                return type;
            }
            
            internal MethodBase Map(MethodBase method) {
                //a given method must be reduced to its basic form, without generic args

                Type[] genArgs = null;

                if(method is MethodInfo) {
                    var methodInfo = (MethodInfo)method;

                    if(methodInfo.IsGenericMethod) {
                        genArgs = methodInfo.GetGenericArguments()
                                                .Select(t => Map(t))
                                                .ToArray();

                        method = methodInfo.GetGenericMethodDefinition();
                    }
                }


                Type[] contGenArgs = null;

                if(method.DeclaringType.IsGenericType) {
                    contGenArgs = method.DeclaringType.GetGenericArguments()
                                                        .Select(t => Map(t))
                                                        .ToArray();

                    method = method.DeclaringType.GetGenericTypeDefinition()
                                                    .GetMethods()
                                                    .First(m => method.Name == m.Name); // MethodComparer.Instance.Equals(method, m));
                }
                
                                
                MethodBase foundMethod;

                if(_dMethods.TryGetValue(method, out foundMethod)) {
                    method = foundMethod;
                }


                if(contGenArgs != null) {
                    method = method.DeclaringType.MakeGenericType(contGenArgs)
                                                    .GetMethods()
                                                    .First(m => m.Name == method.Name); // MethodComparer.Instance.Equals(method, m));
                }


                if(genArgs != null) {
                    method = ((MethodInfo)method).MakeGenericMethod(genArgs);
                }

                return method;
            }

            internal MemberInfo Map(MemberInfo member) {
                MemberInfo foundMember;

                //should do same as with methods...

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
                    if(x is MethodBase && y is MethodBase) {
                        return MethodComparer.Instance.Equals((MethodBase)x, (MethodBase)y);
                    }

                    return TypeComparer.Instance.Equals(x.DeclaringType, y.DeclaringType)
                            && x.Name == y.Name;
                }

                public int GetHashCode(MemberInfo obj) {
                    return (obj.DeclaringType.FullName + obj.Name).GetHashCode();
                }
            }

            class MethodComparer : IEqualityComparer<MethodBase>
            {
                public static readonly MethodComparer Instance = new MethodComparer();

                public bool Equals(MethodBase x, MethodBase y) {
                    return x.Name == y.Name
                            && TypeComparer.Instance.Equals(x.DeclaringType, y.DeclaringType)
                            && (!(x is MethodInfo && y is MethodInfo) 
                                || TypeComparer.Instance.Equals(((MethodInfo)x).ReturnType, ((MethodInfo)y).ReturnType))
                            && x.GetParameters().SequenceEqual(y.GetParameters(), ParameterComparer.Instance);
                }

                public int GetHashCode(MethodBase obj) {
                    return (obj.Name + obj.DeclaringType.FullName).GetHashCode();
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
                                    (MethodInfo)method,
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

            protected override MemberAssignment VisitMemberAssignment(MemberAssignment node) {
                return Expression.Bind(
                                    _mapper.Map(node.Member),
                                    Visit(node.Expression)
                                    );
            }

            protected override Expression VisitUnary(UnaryExpression node) {
                return Expression.Convert(
                                    Visit(node.Operand),
                                    _mapper.Map(node.Type)
                                    );
            }

            

        }




        Mapper _mapper;

        public MethodTester(Module newModule) {
            _mapper = new Mapper(newModule);
        }






        public IClassMethodTester<TClass> WithClass<TClass>() {
            return new ClassMethodTester<TClass>(_mapper);
        }


        public IStaticMethodTester Static() {
            return new StaticMethodTester(_mapper);
        }



        public interface IStaticMethodTester
        {
            TResult Run<TResult>(Expression<Func<TResult>> exFn);
        }


        public interface IClassMethodTester<TClass>
        {
            void Run(Expression<Action<TClass>> exFn);
            TResult Run<TResult>(Expression<Func<TClass,TResult>> exFn);
            MethodInfo GetMethod(Func<MethodInfo, bool> fnSelect);
        }


        class StaticMethodTester
            : IStaticMethodTester
        {
            Mapper _mapper;

            public StaticMethodTester(Mapper mapper) {
                _mapper = mapper;
            }

            public TResult Run<TResult>(Expression<Func<TResult>> exFn) {
                var replacer = new Replacer(_mapper);

                var newEx = (LambdaExpression)replacer.Visit(exFn);
                var newFn = newEx.Compile();

                var result = newFn.DynamicInvoke();

                return (TResult)result;
            }
        }


        class ClassMethodTester<TClass>
            : IClassMethodTester<TClass>
        {
            Mapper _mapper;

            public ClassMethodTester(Mapper mapper) {
                _mapper = mapper;
            }

            public void Run(Expression<Action<TClass>> exFn) {
                var replacer = new Replacer(_mapper);

                var newEx = (LambdaExpression)replacer.Visit(exFn);
                var newFn = newEx.Compile();

                var baseType = _mapper.Map(typeof(TClass));
                object baseObj = Activator.CreateInstance(baseType);

                newFn.DynamicInvoke(baseObj);
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


            public MethodInfo GetMethod(Func<MethodInfo, bool> fnSelect) {
                var baseType = _mapper.Map(typeof(TClass));
                var method = baseType.GetMethods().Single(fnSelect);
                return method;
            }



        }



    }
}
