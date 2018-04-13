using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using DbdocFramework.Abstracts;
using DbdocFramework.DI.Abstract;
using DbdocFramework.DI.Implementation;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Implementation;
using DbdocFramework.MongoDbProvider.Implementation.QueryProviders.EagerLoading;
using DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading;
using DbdocFramework.MongoDbProvider.Implementation.Serializers.SerializationProvider;
using DbdocFramework.MongoDbProvider.Implementation.TypeMetadataInitializer;
using DbdocFramework.MongoDbProvider.Settings;

namespace DbdocFramework.MongoDbProvider
{
    class MongoDbProvider: IProvider, IDbsetContainer
    {
        private IList<object> DbSets { get; }
        private IMongoDatabase Database { get; }
        private ICustomServiceProvider ServiceProvider { get; }
        private ITypeInitializer TypeInititalizer { get; }
        private bool DropCollectionsEachTime { get; }

        static MongoDbProvider()
        {
            BsonSerializer.RegisterSerializationProvider(new CachableSerializationProvider(new TypeInitializerImpl()));
        }

        public MongoDbProvider(MongoDbContextSettings contextSettings)
        {
            DbSets = new List<object>();
            var connection = new MongoUrlBuilder(contextSettings.ConnectionString);
            var client = new MongoClient(contextSettings.ConnectionString);
            this.Database = client.GetDatabase(connection.DatabaseName);
            DropCollectionsEachTime = contextSettings.DropCollectionsEachTime;
            ICustomServiceCollection serviceCollection = new CustomServiceCollection();
            this.ConfigureServices(serviceCollection);
            this.ServiceProvider = serviceCollection.BuildServiceProvider();
            TypeInititalizer = ServiceProvider.GetService<ITypeInitializer>();
        }

        public void RegisterModel<T>() where T: class
        {
            TypeInititalizer.RegisterType<T>();

            if (this.DropCollectionsEachTime)
            {
                Database.DropCollectionAsync(TypeInititalizer.GetTypeMetadata<T>().CollectionName);
            }

            object dbSet = ServiceProvider.GetService<IDbSet<T>>();
            this.DbSets.Add(dbSet);
        }

        public IDbSet<T> GetDbSet<T>() where T : class
        {
            return (IDbSet<T>) DbSets.FirstOrDefault(db => db.GetType().GetInterfaces().Contains(typeof(IDbSet<T>)));
        }

        protected void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<ITypeInitializer, TypeInitializerImpl>()
                .AddSingleton<IMongoDatabase>(this.Database)
                .AddSingleton<IDbsetContainer>(this)
                .AddSingleton(typeof(IDbSet<>), typeof(MongoDbSet<>))
                .AddSingleton<ILazyLoadingInterceptor, LazyLoadingInterceptor>()
                .AddSingleton<ILazyLoadingProxyGenerator, LazyLoadingProxyGenerator>()
                .AddTransient(typeof(LazyLoadingQueryProvider<>))
                .AddTransient(typeof(EagerLoadingQueryProvider<>))
                .AddTransient(typeof(ILazyLoadingIncludableQueryable<>), typeof(LazyLoadingIncludableQueryable<>))
                .AddTransient(typeof(IEagerLoadingIncludableQueryable<>), typeof(EagerLoadingIncludableQueryable<>))
                .AddTransient<IDataLoadersProvider, CacheableDataLoadersProvider>();
        }
    }
}
