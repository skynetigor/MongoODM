using System.Collections.Generic;
using System.Linq;

namespace DbdocFramework.Abstracts
{
    public interface IDbSet<TEntity> : IDataProcessor<TEntity>
        where TEntity : class
    {
        IIncludableQueryable<TEntity> UseLazyLoading();

        IIncludableQueryable<TEntity> UseEagerLoading();
    }
}
