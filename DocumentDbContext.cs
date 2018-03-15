using DbdocFramework.Abstracts;
using System;
using System.Linq;
using System.Reflection;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Settings;

namespace DbdocFramework
{
    public abstract class DocumentDbContext
    {
        private IProvider Provider { get; }

        protected DocumentDbContext(IProvider provider)
        {
            this.Provider = provider;
            Setup();
        }

        protected DocumentDbContext(MongoDbContextSettings mongoSettings)
            : this(new MongoDbProvider.MongoDbProvider(mongoSettings)) { } 

        public IDbSet<T> Set<T>()
            where T : class
        {
            return this.Provider.GetDbSet<T>();
        }

        private void Setup()
        {
            var items = this.GetProperties().Where(p => p.PropertyType.Name == (typeof(IDbSet<>).Name));

            foreach (var prop in items)
            {
                var modelType = prop.PropertyType.GetGenericArguments()[0];
                this.Provider.RegisterModel(modelType);
            }

            foreach (var prop in items)
            {
                var modelType = prop.PropertyType.GetGenericArguments()[0];
                var instance = this.Provider.GetDbSet(modelType);
                prop.SetValue(this, instance);
            }

            this.Provider.InitializeTypesMetadata();
        }
    }
}