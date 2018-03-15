using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace DbdocFramework.Abstracts.Queryable
{
    interface IEagerLoadingQueryProvider<T>: IQueryProvider
    {
        IQueryable<T> CreateQuery(IList<BsonDocument> pipeline);
    }
}
