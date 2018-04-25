using System.Collections.Generic;

namespace DbdocFramework.Abstracts
{
    public interface IDataProcessor<in TEntity> where TEntity: class
    {
        void Add(TEntity entity);

        void AddRange(IEnumerable<TEntity> entities);

        void Update(TEntity entity);

        void UpdateRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);

        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
