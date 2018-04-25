using System.Collections.Generic;
using DbdocFramework.DI.Abstract;
using DbdocFramework.DI.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Implementation.State.Savers;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.Implementation.State
{
    internal class State<TEntity>: IState<TEntity> where TEntity: class 
    {
        private readonly List<IChangesSaver> _savers = new List<IChangesSaver>();

        private AddedDataChangesSaver<TEntity> AddedDataChangesSaver { get; }
        private UpdatedDataChangesSaver<TEntity> UpdatedDataChangesSaver { get; }
        private RemovedDataChangesSaver<TEntity> RemovedDataChangesSaver { get; }
        private ICustomServiceProvider ServiceProvider { get; }

        public State(ICustomServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            AddedDataChangesSaver = CreateChangesSaver<AddedDataChangesSaver<TEntity>>();
            UpdatedDataChangesSaver = CreateChangesSaver<UpdatedDataChangesSaver<TEntity>>();
            RemovedDataChangesSaver = CreateChangesSaver<RemovedDataChangesSaver<TEntity>>();
        }

        public void Add(TEntity entity)
        {
            AddedDataChangesSaver.AddData(entity);
            RemovedDataChangesSaver.RemoveData(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            AddedDataChangesSaver.AddRangeData(entities);
            RemovedDataChangesSaver.RemoveRangeData(entities);
        }

        public void Update(TEntity entity)
        {
            UpdatedDataChangesSaver.AddData(entity);
            RemovedDataChangesSaver.RemoveData(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            UpdatedDataChangesSaver.AddRangeData(entities);
            RemovedDataChangesSaver.RemoveRangeData(entities);
        }

        public void Remove(TEntity entity)
        {
            RemovedDataChangesSaver.AddData(entity);
            AddedDataChangesSaver.RemoveData(entity);
            UpdatedDataChangesSaver.RemoveData(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            RemovedDataChangesSaver.AddRangeData(entities);
            AddedDataChangesSaver.RemoveRangeData(entities);
            UpdatedDataChangesSaver.RemoveRangeData(entities);
        }

        public void SaveChanges() => _savers.ForEach(s => s.SaveChanges());

        private TSaver CreateChangesSaver<TSaver>() where TSaver : AbstractChangesSaver<TEntity>
        {
            TSaver saver = ServiceProvider.CreateInstance<TSaver>();
            _savers.Add(saver);
            return saver;
        }
    }
}
