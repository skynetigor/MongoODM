using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DbdocFramework.Abstracts;
using DbdocFramework.Abstracts.Queryable;
using DbdocFramework.MongoDbProvider.QueryProvider;
using MongoDB.Bson;
using MongoDB.Driver.Linq;

namespace DbdocFramework.MongoDbProvider.LazyLoading
{
    class LazyLoadingQueryable<T>: MongoQueryable<T>, ILazyLoadingQueryable<T>
    {
        public LazyLoadingQueryable(IQueryProvider provider, Expression expression) : base(provider, expression)
        {
        }
    }
}

