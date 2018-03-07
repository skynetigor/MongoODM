using MongoODM.Abstracts;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoODM.Models;
using System.Linq;
using MongoODM.Extensions;
using MongoODM.QueryProvider;

namespace MongoODM.Includables
{
    internal class IncludableEnumerable<TEntity> : IIncludableEnumerable<TEntity>
    {
        private IMongoDatabase database;
        private ITypeInitializer typeInitializer;

        private TypeModel currentTypeModel;

        public IncludableEnumerable(IMongoDatabase database, ITypeInitializer typeInitializer)
        {
            this.database = database;
            this.typeInitializer = typeInitializer;
            this.currentTypeModel = this.typeInitializer.GetTypeModel<TEntity>();
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
            var includableInstance = new IncludableEnumerable<TEntity>(database, typeInitializer);
            var typeEntity = typeof(TEntity);

            var queryList = new List<BsonDocument>();

            foreach (var path in navigationPropsPath)
            {
                var queryCollection = currentTypeModel.QueryDictionary[path];
                if (queryCollection != null)
                {
                    foreach (var doc in queryCollection)
                    {
                        queryList.Add(doc);
                    }
                }
            }

            return this.AsQueryable(queryList);
        }

        private IQueryable<TEntity> AsQueryable(IList<BsonDocument> pipe)
        {
            var queryable = this.database.GetCollection<TEntity>(this.currentTypeModel.CollectionName).AsQueryable();
            return new MongoQueryable<TEntity>(queryable, pipe);
        }

        public IQueryable<TEntity> AsQueryable()
        {
            return this.AsQueryable(null);
        }

        public IQueryable<TEntity> Include()
        {
            var includableInstance = new IncludableEnumerable<TEntity>(database, typeInitializer);
            var typeEntity = typeof(TEntity);
            var queryList = new List<BsonDocument>();

            foreach (var doc in this.currentTypeModel.QueryDictionary.Values)
            {
                queryList.AddRange(doc);
            }

            return this.AsQueryable(queryList);
        }
    }
}
