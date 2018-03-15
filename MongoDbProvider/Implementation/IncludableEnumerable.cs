using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using DbdocFramework.Abstracts;
using DbdocFramework.Abstracts.Queryable;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;
using DbdocFramework.MongoDbProvider.QueryProvider;
using Microsoft.Extensions.DependencyInjection;

namespace DbdocFramework.MongoDbProvider.Implementation
{
    internal class IncludableEnumerable<TEntity> : IIncludableEnumerable<TEntity> where TEntity: class 
    {
        private IMongoDatabase Database { get; }
        private ITypeInitializer TypeInitializer { get; }
        private TypeMetadata CurrentTypeModel => this.TypeInitializer.GetTypeMetadata<TEntity>();
        private ICustomServiceProvider ServiceProvider { get; }

        public IncludableEnumerable(IMongoDatabase database, ITypeInitializer typeInitializer, ICustomServiceProvider serviceProvider)
        {
            this.Database = database;
            this.TypeInitializer = typeInitializer;
            ServiceProvider = serviceProvider;
        }

        public IQueryable<TEntity> Include(params Expression<Func<TEntity, object>>[] navigationPropsPath)
        {
            var p = navigationPropsPath
                .Where(exp => exp.Body is MemberExpression)
                .Select(exp => exp.Body)
                .Cast<MemberExpression>()
                .Select(exp => exp.Member.Name)
                .ToArray();
            return this.Include(p);
        }

        public IQueryable<TEntity> Include(params string[] navigationPropsPath)
        {
            var includableInstance = new IncludableEnumerable<TEntity>(Database, TypeInitializer, ServiceProvider);
            var typeEntity = typeof(TEntity);

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

            return this.ServiceProvider.GetService<IEagerLoadingQueryProvider<TEntity>>().CreateQuery(queryList);
        }

        public IQueryable<TEntity> AsQueryable()
        {
            return this.Database.GetCollection<TEntity>(this.CurrentTypeModel.CollectionName).AsQueryable();
        }

        public IQueryable<TEntity> Include()
        {
            var includableInstance = new IncludableEnumerable<TEntity>(Database, TypeInitializer, ServiceProvider);
            var typeEntity = typeof(TEntity);
            var queryList = new List<BsonDocument>();

            foreach (var doc in this.CurrentTypeModel.QueryDictionary.Values)
            {
                queryList.AddRange(doc);
            }

            return this.ServiceProvider.GetService<IEagerLoadingQueryProvider<TEntity>>().CreateQuery(queryList);
        }
    }
}
