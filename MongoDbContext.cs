using MongoDB.Driver;
using MongoODM.Abstracts;
using MongoODM.ItemsSets;
using System;
using System.Linq;
using MongoODM.Extensions;

namespace MongoODM
{
    public abstract class MongoDbContext
    {
        private readonly IMongoDatabase database;

        protected MongoDbContext(MongoContextSettings contextSettings)
        {
            var connection = new MongoUrlBuilder(contextSettings.ConnectionString);
            var client = new MongoClient(contextSettings.ConnectionString);
            this.database = client.GetDatabase(connection.DatabaseName);
            Setup();
        }

        public IModelsProvider<T> Set<T>()
            where T : class
        {
            var thisType = typeof(T);
            var itemSetType = typeof(IModelsProvider<>);
            var item = this.GetProperties()
                .Where(p => p.PropertyType.Name == itemSetType.Name)
                .FirstOrDefault(p => p.PropertyType.GetGenericArguments()[0] == thisType);

            if (item != null)
            {
                return item.GetValue(this) as IModelsProvider<T>;
            }

            var msg = string.Format("Type \"{0}\" is not sets for this context!", thisType.Name);
            throw new Exception(msg);
        }

        private void Setup()
        {
            var items = this.GetProperties().Where(p => p.PropertyType.Name == typeof(IModelsProvider<>).Name);

            foreach (var prop in items)
            {
                var obj = this.CreateProviderInstance(prop.PropertyType.GetGenericArguments()[0]);
                prop.SetValue(this, obj);
            }

            foreach (var prop in items)
            {
                var val = prop.GetValue(this);
                var queryMethod = val.GetType().GetMethod("InitializeQuery");
                queryMethod.Invoke(val, null);
            }
        }

        private object CreateProviderInstance(Type type)
        {
            var providerType = typeof(MongoDbModelsProvider<>);
            providerType = providerType.MakeGenericType(type);
            return Activator.CreateInstance(providerType, this.database, this);
        }
    }
}