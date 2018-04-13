using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using Microsoft.Extensions.DependencyInjection;
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
            try
            {
                var currenttypeMetadata = this.TypeInitializer.GetTypeMetadata(source.GetType());
                var navPropName = typeof(T).Name;

                var queryList = new List<BsonDocument>
                {
                    new BsonDocument("$match",
                        new BsonDocument("_id", currenttypeMetadata.IdProperty.GetValue(source).ToString()))
                };

                queryList.AddRange(currenttypeMetadata.QueryDictionary[loadedProperty.Name]);
                queryList.Add(new BsonDocument("$replaceRoot", new BsonDocument
                {
                    { "newRoot", $"${navPropName}" }
                }));

                PipelineDefinition<TSource, T> pipeline = queryList;
                var resultTarget = this.Database.GetCollection<TSource>(currenttypeMetadata.CollectionName).Aggregate(pipeline).FirstOrDefault();

                if (resultTarget != null)
                {
                    return this.ServiceProvider.GetService<ILazyLoadingProxyGenerator>().CreateProxy(resultTarget);
                }
                return default(T);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
