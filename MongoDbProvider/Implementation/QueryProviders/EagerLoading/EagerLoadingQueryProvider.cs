using DbdocFramework.MongoDbProvider.Abstracts;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.EagerLoading
{
    internal class EagerLoadingQueryProvider<T>: AbstractQueryProviderFromPipeline<T>
    {
        public EagerLoadingQueryProvider(IMongoDatabase database, ITypeInitializer typeInitializer) : base(database, typeInitializer)
        {
        }
    }
}
