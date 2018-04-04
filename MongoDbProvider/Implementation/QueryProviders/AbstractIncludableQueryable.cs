using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Castle.DynamicProxy;
using DbdocFramework.Abstracts;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders
{
    abstract class AbstractIncludableQueryable<T>: IIncludableQueryable<T> where T: class
    {
        private Expression expression;

        private IMongoDatabase Database { get; }
        protected ITypeInitializer TypeInitializer { get; }
        protected TypeMetadata CurrentTypeModel => this.TypeInitializer.GetTypeMetadata<T>();
        protected abstract IQueryProviderFromPipeline<T> QueryProviderFromPipeline { get; }

        public Type ElementType => typeof(T);

        public Expression Expression
        {
            get
            {
                if (expression == null)
                {
                    expression = Database.GetCollection<T>(CurrentTypeModel.CollectionName).AsQueryable().Expression;
                }

                return expression;
            }
        }

        public IQueryProvider Provider => this.QueryProviderFromPipeline;

        protected AbstractIncludableQueryable(ITypeInitializer typeInitializer, IMongoDatabase database)
        {
            this.TypeInitializer = typeInitializer;
            this.Database = database;
        }

        public IQueryable<T> Include(params Expression<Func<T, object>>[] navigationPropsPath)
        {
            var p = navigationPropsPath
                .Where(exp => exp.Body is MemberExpression)
                .Select(exp => exp.Body)
                .Cast<MemberExpression>()
                .Select(exp => exp.Member.Name)
                .ToArray();

            return this.Include(p);
        }

        public IQueryable<T> Include(params string[] navigationPropsPath)
        {
            var queryList = new List<BsonDocument>();

            foreach (var path in navigationPropsPath)
            {
                var queryCollection = CurrentTypeModel.QueryDictionary[path];
                if (queryCollection != null)
                {
                    foreach (var doc in queryCollection)
                    {
                        queryList.Add(doc);
                    }
                }
            }

            return this.QueryProviderFromPipeline.CreateQueryFromPipeline(queryList);
        }

        public IQueryable<T> Include()
        {
            var queryList = new List<BsonDocument>();

            foreach (var doc in this.CurrentTypeModel.QueryDictionary.Values)
            {
                queryList.AddRange(doc);
            }

            return this.QueryProviderFromPipeline.CreateQueryFromPipeline(queryList);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.Provider.Execute(this.Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
