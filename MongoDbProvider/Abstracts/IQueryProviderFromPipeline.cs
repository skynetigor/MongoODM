using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    interface IQueryProviderFromPipeline<T>: IQueryProvider
    {
        IQueryable<T> CreateQueryFromPipeline(IList<BsonDocument> pipeline);
    }
}
