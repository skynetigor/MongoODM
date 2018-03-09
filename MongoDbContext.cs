using MongoDB.Driver;
using MongoODM.Abstracts;
using MongoODM.ItemsSets;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MongoODM.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoODM.DI.Abstract;
using MongoODM.DI.Implementation;
using MongoODM.Serializers;

namespace MongoODM
{
    public abstract class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private ICustomServiceProvider _serviceProvider;

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
           var modelsProvider = this.GetType().GetProperties().FirstOrDefault(prop => prop.PropertyType == typeof(IModelsProvider<T>));

            return modelsProvider != null ? (IModelsProvider<T>)modelsProvider.GetValue(this) : null;
        }

        private void Setup()
        {
            ICustomServiceCollection serviceCollection = new CustomServiceCollection();
            this.ConfigureServices(serviceCollection);
            this._serviceProvider = serviceCollection.BuildServiceProvider();

            var items = this.GetProperties().Where(p => p.PropertyType.Name == (typeof(IModelsProvider<>).Name));

            foreach (var prop in items)
            {
                var instance = _serviceProvider.CreateInstance(MakeGenericTypeModelsProviderImplementation(prop.PropertyType));
                prop.SetValue(this, instance);
                //serviceCollection.AddSingleton(prop.PropertyType, MakeGenericTypeModelsProviderImplementation(prop.PropertyType));
            }


            IClassMapper classMapper = this._serviceProvider.GetService<IClassMapper>();
            IQueryInitializer queryInitializer = this._serviceProvider.GetService<IQueryInitializer>();

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
                .AddSingleton<IBsonSerializationProvider, SerializationProvider>()
                .AddTransient<IClassMapper, ClassMapper>()
                .AddTransient<IQueryInitializer, QueryInitializer>();
        }

    }
}