using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DbdocFramework.Abstracts
{
    public interface IIncludableEnumerable<TEntity> where TEntity: class 
    {
        IQueryable<TEntity> Include();

        IQueryable<TEntity> Include(params Expression<Func<TEntity, object>>[] navigationPropsPath);

        IQueryable<TEntity> Include(params string[] navigationPropsPath);

        IQueryable<TEntity> AsQueryable();

    }
}
