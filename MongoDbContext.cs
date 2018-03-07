using MongoDB.Driver;
using MongoODM.Abstracts;
using MongoODM.ItemsSets;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MongoODM.Extensions;

namespace MongoODM
{
    public abstract class MongoDbContext
    {
        public readonly IMongoDatabase _database;
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
                this._serviceCollection.AddSingleton(MakeGenericTypeModelsProviderInterface(prop.PropertyType), MakeGenericTypeModelsProviderImplementation(prop.PropertyType));
            }

            this._serviceProvider = this._serviceCollection.BuildServiceProvider();

            foreach (var prop in items)
            {
                prop.SetValue(this, this._serviceProvider.GetService(MakeGenericTypeModelsProviderInterface(prop.PropertyType)));
            }

            foreach (var prop in items)
            {
                var val = prop.GetValue(this);
                var queryMethod = val.GetType().GetMethod("InitializeQuery");
                queryMethod.Invoke(val, null);
            }
        }

        private Type MakeGenericTypeModelsProviderInterface(Type argument)
        {
            return typeof(IModelsProvider<>).MakeGenericType(argument.GetGenericArguments()[0]);
        }

        private Type MakeGenericTypeModelsProviderImplementation(Type argument)
        {
            return typeof(MongoDbModelsProvider<>).MakeGenericType(argument.GetGenericArguments()[0]);
        }

        protected virtual void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITypeInitializer, TypeInitializer>();
            serviceCollection.AddSingleton<IMongoDatabase>(this._database);
            serviceCollection.AddSingleton<MongoDbContext>(this);
            serviceCollection.AddSingleton<IClassMapper, ClassMapper>();
            serviceCollection.AddSingleton<IQueryInitializer, QueryInitializer>();
        }

    }
}