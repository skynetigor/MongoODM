using DbdocFramework.MongoDbProvider.Abstracts;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.QueryProviders.EagerLoading
{
    internal class EagerLoadingQueryProvider<T>: AbstractQueryProviderFromPipeline<T>
    {
        public EagerLoadingQueryProvider(IMongoDatabase database, ITypeInitializer typeInitializer) : base(database, typeInitializer)
        {
        }
    }
}
