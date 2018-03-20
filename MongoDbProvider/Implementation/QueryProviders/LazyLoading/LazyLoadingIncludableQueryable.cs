using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading
{
    class LazyLoadingIncludableQueryable<T> : AbstractIncludableQueryable<T>, ILazyLoadingIncludableQueryable<T> where T: class 
    {
        private IQueryProviderFromPipeline<T> queryProvider;
        private ICustomServiceProvider ServiceProvider { get; }

        public LazyLoadingIncludableQueryable(ITypeInitializer typeInitializer, ICustomServiceProvider serviceProvider, IMongoDatabase database) : base(typeInitializer, database)
        {
            this.ServiceProvider = serviceProvider;
        }

        protected override IQueryProviderFromPipeline<T> QueryProviderFromPipeline
        {
            get
            {
                if (queryProvider == null)
                {
                    queryProvider = this.ServiceProvider.GetService<LazyLoadingQueryProvider<T>>();
                }

                return queryProvider;
            }
        }
    }
}
