using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using Microsoft.Extensions.DependencyInjection;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading
{
    class LazyLoadingProxyGenerator : ILazyLoadingProxyGenerator
    {
        private IProxyGenerator ProxyGenerator { get; }
        private ICustomServiceProvider ServiceProvider { get; }
        private ITypeInitializer TypeInitializer { get; }

        public LazyLoadingProxyGenerator(ICustomServiceProvider serviceProvider, ITypeInitializer typeInitializer)
        {
            this.ProxyGenerator = new ProxyGenerator();
            this.ServiceProvider = serviceProvider;
            this.TypeInitializer = typeInitializer;
        }

        private object CreateProxyHelper(object target)
        {
            if (target == null)
            {
                return null;
            }

            var targetType = target.GetType();
            var interceptor = ServiceProvider.GetService<ILazyLoadingInterceptor>();
            var proxy = ProxyGenerator.CreateClassProxyWithTarget(targetType, target, interceptor);

            foreach (var propertyInfo in target.GetType().GetProperties())
            {
                bool isVirtual = propertyInfo.GetGetMethod().IsVirtual;

                if (IsTypeRegistered(propertyInfo.PropertyType) && isVirtual)
                {
                    var value = propertyInfo.GetValue(target);

                    propertyInfo.SetValue(target, CreateProxyHelper(value));
                }
                else
                {
                    propertyInfo.SetValue(proxy, propertyInfo.GetValue(target));
                }
            }

            return proxy;
        }

        private bool IsTypeRegistered(Type type)
        {
            if (type.IsGenericType)
            {
                var genericDefinition = type.GetGenericTypeDefinition();

                if (genericDefinition == typeof(ICollection<>) || genericDefinition == typeof(IList<>))
                {
                    type = type.GetGenericArguments()[0];
                }
            }

            return TypeInitializer.IsTypeRegistered(type);
        }

        public T CreateProxy<T>(T target)
        {
            return (T)CreateProxyHelper(target);
        }

        public IEnumerable<T> CreateProxies<T>(IEnumerable<T> targets)
        {
            return targets.Select(CreateProxy);
        }
    }
}
