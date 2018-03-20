using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Helpers;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading.Loaders
{
    class EnumerableDataLoader<TResult> : IDataLoader<IEnumerable<TResult>>
    {
        private IMongoDatabase Database { get; }
        private ITypeInitializer TypeInitializer { get; }
        private IMongoDbLazyLoadingInterceptor Interceptor { get; }

        public EnumerableDataLoader(IMongoDatabase database, ITypeInitializer typeInitializer, IMongoDbLazyLoadingInterceptor interceptor)
        {
            Database = database;
            TypeInitializer = typeInitializer;
            this.Interceptor = interceptor;
        }

        public IEnumerable<TResult> LoadData<TSource>(TSource source, PropertyInfo loadedProperty)
        {
            var resultTypeMetadata = this.TypeInitializer.GetTypeMetadata<TResult>();

            var navPropName = typeof(TResult).Name;

            var navigationProperty = typeof(TResult).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(TSource));

            var queryList = new List<BsonDocument>
            {
                new BsonDocument("$match",
                    new BsonDocument(navigationProperty.GetNavigationPropertyName(), resultTypeMetadata.IdProperty.GetValue(source).ToString())),
                //new BsonDocument("$project", new BsonDocument
                //{
                //    {"result", "$$ROOT"},
                //    {"_id", 0}
                //})
            };

            PipelineDefinition<TResult, TResult> pipeline = queryList;

            var targets = this.Database.GetCollection<TResult>(resultTypeMetadata.CollectionName).Aggregate(pipeline).ToEnumerable();

            return this.Interceptor.CreateProxies(targets);
        }
    }
}
