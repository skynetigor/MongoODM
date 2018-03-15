using System.Collections.Generic;
using System.Linq;
using DbdocFramework.Abstracts.Queryable;

namespace DbdocFramework.Abstracts
{
    public interface IDbSet<TEntity> : IEnumerable<TEntity>, IIncludableEnumerable<TEntity>
        where TEntity: class
    {
        void Add(TEntity entity);

        void AddRange(IEnumerable<TEntity> entities);

        void Update(TEntity entity);

        void UpdateRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);

        void RemoveRange(IEnumerable<TEntity> entities);

        ILazyLoadingQueryable<TEntity> UseLazyLoading();
    }
}
