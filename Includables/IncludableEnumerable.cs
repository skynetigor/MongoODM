using MongoORM.Abstracts;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoORM.Models;
using System.Linq;

namespace MongoORM.Includables
{
    internal class IncludableEnumerable<TEntity> : IIncludableEnumerable<TEntity>
    {
        private IMongoDatabase database;
        private ITypeInitializer typeInitializer;
        private PipelineDefinition<TEntity, TEntity> pipelineDefinition;
        private TypeModel currentTypeModel;

        public IncludableEnumerable(IMongoDatabase database, ITypeInitializer typeInitializer)
        {
            this.database = database;
            this.typeInitializer = typeInitializer;
            this.currentTypeModel = this.typeInitializer.GetTypeModel<TEntity>();
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            if (pipelineDefinition == null)
            {
                this.pipelineDefinition = new BsonDocument[0];
            }

            return this.database.GetCollection<TEntity>(this.currentTypeModel.CollectionName)
                .Aggregate(this.pipelineDefinition)
                .ToEnumerable<TEntity>()
                .GetEnumerator();
        }

        public IEnumerable<TEntity> Include(params Expression<Func<TEntity, object>>[] navigationPropsPath)
        {
            var p = navigationPropsPath
                .Where(exp => exp.Body is MemberExpression)
                .Select(exp => exp.Body)
                .Cast<MemberExpression>()
                .Select(exp => exp.Member.Name)
                .ToArray();
            return this.Include(p);
        }

        public IEnumerable<TEntity> Include(params string[] navigationPropsPath)
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

            includableInstance.pipelineDefinition = queryList;
            return includableInstance;
        }

        public IEnumerable<TEntity> Include()
        {
            var includableInstance = new IncludableEnumerable<TEntity>(database, typeInitializer);
            var typeEntity = typeof(TEntity);
            var queryList = new List<BsonDocument>();

            foreach (var doc in this.currentTypeModel.QueryDictionary.Values)
            {
                queryList.AddRange(doc);
            }

            includableInstance.pipelineDefinition = queryList;
            return includableInstance;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }
}
