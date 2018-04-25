using System.Collections.Generic;
using System.Linq;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Extensions;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.Implementation.State.Savers
{
    internal abstract class  AbstractChangesSaver<TEntity>: IChangesSaver
    {
        private const int PackageCapacity = 100;

        private readonly List<TEntity> _dataForProcessing = new List<TEntity>();

        protected TypeMetadata CurrentTypeModel { get; }
        protected IMongoDatabase Database { get; }
        protected ITypeInitializer TypeInitializer { get; }


        protected AbstractChangesSaver(IMongoDatabase database, ITypeInitializer typeInitializer)
        {
            Database = database;
            TypeInitializer = typeInitializer;
            CurrentTypeModel = typeInitializer.GetTypeMetadata<TEntity>();
        }

        public void SaveChanges()
        {
            if (_dataForProcessing.Any())
            {
                var package = new List<TEntity>();
                int iteration = 0;

                _dataForProcessing.ForEach(e =>
                {
                    if (iteration > 0 && iteration % PackageCapacity == 0)
                    {
                        SaveChanges(package);
                        package.Clear();
                    }

                    if (e != null)
                    {
                        package.Add(e);
                        iteration++;
                    }
                });

                SaveChanges(package);
                _dataForProcessing.Clear();
            }
        }

        public void AddData(TEntity entity)
        {
            BeforeAddData(entity);
            _dataForProcessing.Add(entity);
            AfterAddData(entity);
        }

        public void AddRangeData(IEnumerable<TEntity> entities)
        {
            BeforeAddRangeData(entities);
            _dataForProcessing.AddRange(entities);
            AfterAddRangeData(entities);
        }

        public void RemoveData(TEntity entity) => _dataForProcessing.Remove(entity);

        public void RemoveRangeData(IEnumerable<TEntity> entities) => _dataForProcessing.RemoveRange(entities);

        protected virtual void BeforeAddData(TEntity entity)
        {

        }

        protected virtual void AfterAddData(TEntity entity)
        {

        }

        protected virtual void BeforeAddRangeData(IEnumerable<TEntity> entities)
        {

        }

        protected virtual void AfterAddRangeData(IEnumerable<TEntity> entities)
        {

        }

        protected abstract void SaveChanges(IEnumerable<TEntity> dataForSaving);
    }
}
