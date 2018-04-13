using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    internal interface IQueryProviderFromPipeline<T>: IQueryProvider
    {
        IQueryable<T> CreateQueryFromPipeline(IList<BsonDocument> pipeline);
    }
}
