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
            var interceptor = this.ServiceProvider.GetService<ILazyLoadingInterceptor>();
            var proxy = ProxyGenerator.CreateClassProxyWithTarget(targetType, target, interceptor);

            foreach (var propertyInfo in target.GetType().GetProperties())
            {
                if (!propertyInfo.GetGetMethod().IsVirtual)
                {
                    propertyInfo.SetValue(proxy, propertyInfo.GetValue(target));
                }
                else if(this.TypeInitializer.IsTypeRegistered(propertyInfo.PropertyType))
                {
                    var value = propertyInfo.GetValue(target);

                    if (value != null)
                    {
                       propertyInfo.SetValue(target, this.CreateProxyHelper(value));
                    }
                }
            }

            return proxy;
        }

        public T CreateProxy<T>(T target)
        {
            return (T)this.CreateProxyHelper(target);
        }

        public IEnumerable<T> CreateProxies<T>(IEnumerable<T> targets)
        {
            return targets.Select(this.CreateProxy);
        }
    }
}
