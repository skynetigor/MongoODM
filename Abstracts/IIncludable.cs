using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MongoORM.Abstracts
{
    public interface IIncludableEnumerable<TEntity> : IEnumerable<TEntity>
    {
        IEnumerable<TEntity> Include();

        IEnumerable<TEntity> Include(params Expression<Func<TEntity, object>>[] navigationPropsPath);

        IEnumerable<TEntity> Include(params string[] navigationPropsPath);

    }
}
