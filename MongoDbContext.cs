using Microsoft.Win32.SafeHandles;
using MongoDB.Driver;
using MongoORM.Abstracts;
using MongoORM.ItemsSets;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace MongoORM
{
    public class MongoDbContext
    {
        private IMongoDatabase database;

        public MongoDbContext(MongoContextSettings contextSettings)
        {
            var connection = new MongoUrlBuilder(contextSettings.ConnectionString);
            var client = new MongoClient(contextSettings.ConnectionString);
            this.database = client.GetDatabase(connection.DatabaseName);
            Setup();
        }

        public IModelsProvider<T> Set<T>()
            where T : class
        {
            var contextType = this.GetType();
            var thisType = typeof(T);
            var itemSetType = typeof(IModelsProvider<>);
            var item = contextType.GetProperties()
                .Where(p => p.PropertyType.Name == itemSetType.Name)
                .FirstOrDefault(p => p.PropertyType.GetGenericArguments()[0] == thisType);

            if (item != null)
            {
                return item.GetValue(this) as IModelsProvider<T>;
            }

            var msg = string.Format("Type \"{0}\" is not sets to this context!", thisType.Name);
            throw new Exception(msg);
        }

        private void Setup()
        {
            var contextType = this.GetType();
            var itemSetType = typeof(IModelsProvider<>);
            var items = contextType.GetProperties().Where(p => p.PropertyType.Name == itemSetType.Name);

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