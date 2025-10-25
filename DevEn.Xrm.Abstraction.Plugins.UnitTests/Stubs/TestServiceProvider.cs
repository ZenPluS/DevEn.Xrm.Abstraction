using System;
using System.Collections.Generic;

namespace DevEn.Xrm.Abstraction.Plugins.UnitTests.Stubs
{
    public class TestServiceProvider
        : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void AddService<T>(T instance)
        {
            _services[typeof(T)] = instance;
        }

        public object GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var value);
            return value;
        }
    }
}