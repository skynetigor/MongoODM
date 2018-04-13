using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DbdocFramework.Abstracts;

namespace DbdocFramework.Extensions
{
    public static class DbSetExtension
    {
        public static async Task AddAsync<TEntity>(this IDbSet<TEntity> modelsProvider, TEntity entity,
            CancellationToken cancellationToken = default(CancellationToken))
            where TEntity : class
        {
            await Task.Run(() => modelsProvider.Add(entity), cancellationToken);
        }

        public static async Task AddRangeAsync<TEntity>(this IDbSet<TEntity> modelsProvider,
            IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken))
            where TEntity : class
        {
            await Task.Run(() => modelsProvider.AddRange(entities), cancellationToken);
        }

        public static async Task UpdateAsync<TEntity>(this IDbSet<TEntity> modelsProvider, TEntity entity,
            CancellationToken cancellationToken = default(CancellationToken))
            where TEntity : class
        {
            await Task.Run(() => modelsProvider.Update(entity), cancellationToken);
        }

        public static async Task UpdateRangeAsync<TEntity>(this IDbSet<TEntity> modelsProvider,
            IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken))
            where TEntity : class
        {
            await Task.Run(() => modelsProvider.UpdateRange(entities), cancellationToken);
        }

        public static async Task RemoveAsync<TEntity>(this IDbSet<TEntity> modelsProvider, TEntity entity,
            CancellationToken cancellationToken = default(CancellationToken))
            where TEntity : class
        {
            await Task.Run(() => modelsProvider.Remove(entity), cancellationToken);
        }

        public static async Task RemoveRangeAsync<TEntity>(this IDbSet<TEntity> modelsProvider,
            IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken))
            where TEntity : class
        {
            await Task.Run(() => modelsProvider.RemoveRange(entities), cancellationToken);
        }
    }
}
