using System.Collections.Generic;
using System.Reflection;
using Castle.DynamicProxy;
using DbdocFramework.DI.Abstract;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.QueryProviders.LazyLoading
{
    internal class LazyLoadingInterceptor : IMongoDbLazyLoadingInterceptor
    {
        private IMongoDatabase Database { get; }
        private ITypeInitializer TypeMetadata { get; }
        private ICustomServiceProvider ServiceProvider { get; }
        private ProxyGenerator ProxyGenerator { get; }


        public LazyLoadingInterceptor(IMongoDatabase database, ITypeInitializer typeMetadata, ICustomServiceProvider serviceProvider)
        {
            Database = database;
            TypeMetadata = typeMetadata;
            ServiceProvider = serviceProvider;
            ProxyGenerator = new ProxyGenerator();
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name.StartsWith("get_"))
            {
                var memberTypeMetadata = this.TypeMetadata.GetTypeMetadata(invocation.Method.ReturnType);
                var value = invocation.MethodInvocationTarget.Invoke(invocation.InvocationTarget, invocation.Arguments);
                if (value == null && memberTypeMetadata != null)
                {
                    var invokedProperty = invocation.TargetType.GetProperty(invocation.Method.Name.Substring(4));
                    value = this.GetModel(invocation.InvocationTarget, memberTypeMetadata, invokedProperty);

                    invokedProperty.SetValue(invocation.InvocationTarget, value);
                }

                invocation.ReturnValue = value;
            }

            invocation.Proceed();
        }

        public T CreateProxy<T>(T target) where T : class
        {
            return ProxyGenerator.CreateClassProxyWithTarget(target, new LazyLoadingInterceptor(Database, TypeMetadata, ServiceProvider));
        }

        private object GetModel(object obj, TypeMetadata currentTypeModel, PropertyInfo invokedProp)
        {
            return this.GetPrivateMethod(nameof(GetModelGeneric))
                 .MakeGenericMethod(obj.GetType(), invokedProp.PropertyType)
                 .Invoke(this,
                     new object[] { obj, currentTypeModel, invokedProp });
        }

        private object GetModelGeneric<T, M>(object obj, TypeMetadata currentTypeModel, PropertyInfo invokedProp) where T : class
        {
            var currenttypeMetadata = this.TypeMetadata.GetTypeMetadata(obj.GetType());
            var navPropName = typeof(M).Name;

            var queryList = new List<BsonDocument>
            {
                new BsonDocument("$match",
                    new BsonDocument("_id", currenttypeMetadata.IdProperty.GetValue(obj).ToString()))
            };

            queryList.AddRange(currenttypeMetadata.QueryDictionary[invokedProp.Name]);
            queryList.Add(new BsonDocument("$project", new BsonDocument
            {
                { navPropName, $"${navPropName}" }, { "_id", 0 }
            }));

            PipelineDefinition<T, LazyLoadingContainer<M>> a = queryList;

            var g = this.Database.GetCollection<T>(currenttypeMetadata.CollectionName).Aggregate(a).FirstOrDefault();

            return g.Proxy;
        }
    }
}
