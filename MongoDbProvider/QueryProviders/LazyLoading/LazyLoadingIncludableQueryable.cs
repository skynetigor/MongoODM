using System;
using System.Collections.Generic;
using System.Text;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.QueryProviders.EagerLoading;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.QueryProviders.LazyLoading
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
