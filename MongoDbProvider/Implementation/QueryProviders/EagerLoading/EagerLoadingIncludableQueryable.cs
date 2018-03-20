using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.EagerLoading
{
    class EagerLoadingIncludableQueryable<T> : AbstractIncludableQueryable<T>, IEagerLoadingIncludableQueryable<T> where T: class
    {
        private IQueryProviderFromPipeline<T> queryProvider;

        private ICustomServiceProvider ServiceProvider { get; }

        protected override IQueryProviderFromPipeline<T> QueryProviderFromPipeline
        {
            get
            {
                if (queryProvider == null)
                {
                    queryProvider = this.ServiceProvider.GetService<EagerLoadingQueryProvider<T>>();
                }

                return queryProvider;
            }
        }

        public EagerLoadingIncludableQueryable(ITypeInitializer typeInitializer, ICustomServiceProvider serviceProvider, IMongoDatabase database) : base(typeInitializer, database)
        {
            this.ServiceProvider = serviceProvider;
        }
    }
}
