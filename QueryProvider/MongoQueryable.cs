using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;

namespace MongoODM.QueryProvider
{
    class MongoQueryable<T> : IQueryable<T>
    {
        public Type ElementType => typeof(T);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        public MongoQueryable(IQueryable<T> queryable, IList<BsonDocument> pipeLine)
        {
            this.Provider = new MongoQueryProvider<T>(queryable.Provider, pipeLine);
            this.Expression = queryable.Expression;
        }

        public MongoQueryable(IQueryProvider provider, Expression expression)
        {
            this.Provider = provider;
            this.Expression = expression;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>) this.Provider.Execute(this.Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
