using System.Collections.Generic;
using System.Reflection;
using DbdocFramework.DI.Abstract;
using DbdocFramework.DI.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading.Loaders
{
    abstract class AbstractDataLoader<TModel, TResult> : IDataLoader<TResult>
    {
        protected ICustomServiceProvider ServiceProvider { get; }

        protected IDataLoader<IEnumerable<TModel>> EnumerableDataLoader =>
            this.ServiceProvider.CreateInstance<EnumerableDataLoader<TModel>>();

        protected AbstractDataLoader(ICustomServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public abstract TResult LoadData<TSource>(TSource source, PropertyInfo loadedProperty);
    }
}
