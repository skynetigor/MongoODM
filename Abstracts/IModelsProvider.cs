using System.Collections.Generic;

namespace MongoORM.Abstracts
{
    public interface IModelsProvider<TEntity> : IEnumerable<TEntity>, IIncludableEnumerable<TEntity>
        where TEntity: class
    {
        void InitializeQuery();

        void Add(TEntity entity);

        void AddRange(IEnumerable<TEntity> entities);

        void Update(TEntity entity);

        void UpdateRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);

        void RemoveRange(IEnumerable<TEntity> entities);

    }
}
