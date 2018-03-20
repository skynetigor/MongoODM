using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DbdocFramework.MongoDbProvider.Abstracts;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading
{
    internal class LazyLoadingQueryProvider<T>: AbstractQueryProviderFromPipeline<T> where T: class 
    {
        private IMongoDbLazyLoadingInterceptor Interceptor { get; }

        public LazyLoadingQueryProvider(IMongoDatabase database, ITypeInitializer typeInitializer, IMongoDbLazyLoadingInterceptor interceptor) : base(database, typeInitializer)
        {
            this.Interceptor = interceptor;
        }

        public override object Execute(Expression expression)
        {
            var data = base.Execute(expression);

            if (data is IEnumerable<T> enumerable)
            {
                return enumerable.Select(this.Interceptor.CreateProxy);
            }

            return this.Interceptor.CreateProxy((T)data);
        }
    }
}
