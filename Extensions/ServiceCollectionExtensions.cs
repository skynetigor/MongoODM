using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbdocFramework.Abstracts;
using Microsoft.Extensions.DependencyInjection;

namespace DbdocFramework.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterDocumentDbContext<TContext>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime, object[] ctorParameters) where TContext: DocumentDbContext
        {
            Type contextType = typeof(TContext);

            var ctor = contextType.GetConstructors().FirstOrDefault(c =>
            {
                var b = c.GetParameters();
                   return c.GetParameters().Contains(t => t.ParameterType == typeof(Func<DocumentDbOptionsBuilder, IProvider>));
                });

            serviceCollection.Add(new ServiceDescriptor(contextType, sp => ctor.Invoke(ctorParameters), serviceLifetime));

            return serviceCollection;
        }

        public static IServiceCollection RegisterDocumentDbContext<TContext>(this IServiceCollection serviceCollection,
            ServiceLifetime serviceLifetime, Func<DocumentDbOptionsBuilder, IProvider> builderFunc) where TContext : DocumentDbContext
        {
            return RegisterDocumentDbContext<TContext>(serviceCollection, serviceLifetime, new object[] {builderFunc} );
        }

    }
}
