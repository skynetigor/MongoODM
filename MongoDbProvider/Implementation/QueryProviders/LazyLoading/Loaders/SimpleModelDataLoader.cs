using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DbdocFramework.DI.Abstract;
using DbdocFramework.DI.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading.Loaders
{
    class SimpleModelDataLoader<T> : IDataLoader<T>
    {
        private ICustomServiceProvider ServiceProvider { get; }
        private ITypeInitializer TypeInitializer { get; }
        private IMongoDatabase Database { get; }
        
        public SimpleModelDataLoader(ICustomServiceProvider serviceProvider, ITypeInitializer typeInitializer, IMongoDatabase database)
        {
            this.ServiceProvider = serviceProvider;
            this.TypeInitializer = typeInitializer;
            this.Database = database;
        }

        public T LoadData<TSource>(TSource source, PropertyInfo loadedProperty)
        {
            var currenttypeMetadata = this.TypeInitializer.GetTypeMetadata(source.GetType());
            var navPropName = typeof(T).Name;

            var queryList = new List<BsonDocument>
            {
                new BsonDocument("$match",
                    new BsonDocument("_id", currenttypeMetadata.IdProperty.GetValue(source).ToString()))
            };

            queryList.AddRange(currenttypeMetadata.QueryDictionary[loadedProperty.Name]);
            queryList.Add(new BsonDocument("$project", new BsonDocument
            {
                { "result", $"${navPropName}" }, { "_id", 0 }
            }));

            PipelineDefinition<TSource, LazyLoadingResult<T>> a = queryList;

            var g = this.Database.GetCollection<TSource>(currenttypeMetadata.CollectionName).Aggregate(a).FirstOrDefault();

            return g.Result.FirstOrDefault();
        }
    }
}
