using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbdocFramework.DI.Abstract;
using DbdocFramework.DI.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading.Loaders
{
    class DataLoadersProvider : IDataLoadersProvider
    {
        private ICustomServiceProvider ServiceProvider { get; }
        private ITypeInitializer TypeInitializer { get; }

        public DataLoadersProvider(ICustomServiceProvider serviceProvider, ITypeInitializer typeInitializer)
        {
            this.ServiceProvider = serviceProvider;
            this.TypeInitializer = typeInitializer;
        }

        public IDataLoader<TResult> GetDataLoader<TResult>()
        {
            var resultType = typeof(TResult);

            if (resultType.GetInterfaces().FirstOrDefault(t => t.Name == typeof(IEnumerable<>).Name) != null)
            {
                var genericArgumentType = resultType.GenericTypeArguments[0];

                if (resultType.Name == typeof(ICollection<>).Name)
                {
                    return (IDataLoader<TResult>) this.ServiceProvider.CreateInstance(
                        typeof(CollectionsLoader<>).MakeGenericType(genericArgumentType));
                }
            }

            return this.ServiceProvider.CreateInstance<SimpleModelDataLoader<TResult>>();
        }
    }
}
