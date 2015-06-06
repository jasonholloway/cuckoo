using System;
using System.Reflection;

namespace MethodDecorator.Attributes {
    public interface IMethodDecorator {
        void Init(object instance, MethodBase method, object[] args);
        void OnEntry();
        void OnExit();
        void OnException(Exception exception);
    }
}