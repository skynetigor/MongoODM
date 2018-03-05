using System.Collections.Generic;

namespace MongoORM.Abstracts
{
    public interface IModelSerializer<TResult>
    {
        TResult Serialize<TEntity>(TEntity entity) where TEntity : class;

        IEnumerable<TResult> Serialize<TEntity>(IEnumerable<TEntity> entity) where TEntity : class;

    }
}
