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
using DbdocFramework.MongoDbProvider.Serializers;
using DbdocFramework.DI.Extensions;
using DbdocFramework.MongoDbProvider.Implementation.QueryProviders.EagerLoading;
using DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading;
using DbdocFramework.MongoDbProvider.Implementation.QueryProviders.LazyLoading.Loaders;
using DbdocFramework.MongoDbProvider.Implementation.Serializers;
using DbdocFramework.MongoDbProvider.Settings;

namespace DbdocFramework.MongoDbProvider
{
    class MongoDbProvider: IProvider, IDbsetContainer
    {
        private IList<object> DbSets { get; }
        private IMongoDatabase Database { get; }
        private ICustomServiceProvider ServiceProvider { get; }
        private IClassMapper ClassMapper { get; set; }

        public MongoDbProvider(MongoDbContextSettings contextSettings)
        {
            DbSets = new List<object>();
            var connection = new MongoUrlBuilder(contextSettings.ConnectionString);
            var client = new MongoClient(contextSettings.ConnectionString);
            this.Database = client.GetDatabase(connection.DatabaseName);

            ICustomServiceCollection serviceCollection = new CustomServiceCollection();
            this.ConfigureServices(serviceCollection);
            this.ServiceProvider = serviceCollection.BuildServiceProvider();
            this.ClassMapper = this.ServiceProvider.CreateInstance<ClassMapper>();
        }

        public void RegisterModel<T>() where T: class
        {
            ServiceProvider.GetService<ITypeInitializer>().RegisterType<T>();
            object dbSet = ServiceProvider.GetService<IDbSet<T>>();
            this.DbSets.Add(dbSet);
        }

        public IDbSet<T> GetDbSet<T>() where T : class
        {
            return (IDbSet<T>) DbSets.FirstOrDefault(db => db.GetType().GetInterfaces().Contains(typeof(IDbSet<T>)));
        }

        public void InitializeTypesMetadata()
        {
            var modelsTypes = this.DbSets.Select(db => db.GetType().GetGenericArguments()[0]);
            var queryInitializer = this.ServiceProvider.GetService<IQueryInitializer>();

            foreach (var type in modelsTypes)
            {
                this.ClassMapper.MapClass(type);
                queryInitializer.Initialize(type);
            }

            this.ClassMapper = null;
        }

        protected void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITypeInitializer, TypeInitializer>()
                .AddSingleton<IMongoDatabase>(this.Database)
                .AddSingleton<IDbsetContainer>(this)
                .AddSingleton<IBsonSerializationProvider, SerializationProvider>()
                .AddTransient<IQueryInitializer, QueryInitializer>()
                .AddSingleton(typeof(IDbSet<>), typeof(MongoDbSet<>))
                .AddTransient(typeof(LazyLoadingQueryProvider<>))
                .AddTransient(typeof(EagerLoadingQueryProvider<>))
                .AddTransient(typeof(ILazyLoadingIncludableQueryable<>), typeof(LazyLoadingIncludableQueryable<>))
                .AddTransient(typeof(IEagerLoadingIncludableQueryable<>), typeof(EagerLoadingIncludableQueryable<>))
                .AddSingleton<ILazyLoadingInterceptor, LazyLoadingInterceptor>()
                .AddSingleton<ILazyLoadingProxyGenerator, LazyLoadingProxyGenerator>()
                .AddTransient<IDataLoadersProvider, DataLoadersProvider>();
        }
    }
}
