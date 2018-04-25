using DbdocFramework.Abstracts;
using System.Linq;
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

        public void SaveChanges()
        {
            this.Provider.SaveChanges();
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
            var items = this.GetProperties().Where(p => p.PropertyType.GetGenericTypeDefinition() == typeof(IDbSet<>)).ToArray();

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
        }
    }
}