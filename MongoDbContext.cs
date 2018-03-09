using MongoDB.Driver;
using MongoODM.Abstracts;
using MongoODM.ItemsSets;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MongoODM.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoODM.Serializers;

namespace MongoODM
{
    public abstract class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();
        private IServiceProvider _serviceProvider;

        public bool DropCollectionsWhenContextCreating { get; }

        protected MongoDbContext(MongoContextSettings contextSettings, bool dropCollectionsWhenContextCreating = false)
        {
            var connection = new MongoUrlBuilder(contextSettings.ConnectionString);
            var client = new MongoClient(contextSettings.ConnectionString);
            this._database = client.GetDatabase(connection.DatabaseName);
            this.DropCollectionsWhenContextCreating = dropCollectionsWhenContextCreating;
            Setup();
        }

        public IModelsProvider<T> Set<T>()
            where T : class
        {
            return this._serviceProvider.GetService<IModelsProvider<T>>();
        }

        private void Setup()
        {
            this.ConfigureServices(this._serviceCollection);

            var items = this.GetProperties().Where(p =>  p.PropertyType.Name == (typeof(IModelsProvider<>).Name));

            foreach (var prop in items)
            {
                this._serviceCollection.AddSingleton(prop.PropertyType, MakeGenericTypeModelsProviderImplementation(prop.PropertyType));
            }

            this._serviceProvider = this._serviceCollection.BuildServiceProvider();

            IClassMapper classMapper = this._serviceProvider.GetService<IClassMapper>();
            IQueryInitializer queryInitializer = this._serviceProvider.GetService<IQueryInitializer>();

            foreach (var prop in items)
            {
                prop.SetValue(this, this._serviceProvider.GetService(prop.PropertyType));
            }

            foreach (var prop in items)
            {
                var propertyGenericType = prop.PropertyType.GetGenericArguments()[0];
                classMapper.MapClass(propertyGenericType);
                queryInitializer.Initialize(propertyGenericType);
            }
        }

        private Type MakeGenericTypeModelsProviderImplementation(Type argument)
        {
            return typeof(MongoDbModelsProvider<>).MakeGenericType(argument.GetGenericArguments()[0]);
        }

        protected virtual void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITypeInitializer, TypeInitializer>()
                .AddSingleton<IMongoDatabase>(this._database)
                .AddSingleton<MongoDbContext>(this)
                .AddSingleton<IClassMapper, ClassMapper>()
                .AddSingleton<IQueryInitializer, QueryInitializer>()
                .AddSingleton<IBsonSerializationProvider, SerializationProvider>()
                .AddTransient<IClassMapper, ClassMapper>()
                .AddTransient<IQueryInitializer, QueryInitializer>();
        }

    }
}