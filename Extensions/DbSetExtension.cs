using System.Threading.Tasks;
using DbdocFramework.Abstracts;

namespace DbdocFramework.Extensions
{
    public static class DbSetExtension
    {
        public static void AddAsync<TEntity>(this IDbSet<TEntity> modelsProvider, TEntity entity) where TEntity : class
        {
            modelsProvider.Add(entity);
        }

        public static async void UpdateAsync<TEntity>(this IDbSet<TEntity> modelsProvider, TEntity entity) where TEntity : class
        {
            await Task.Run(() =>
            {
                modelsProvider.Update(entity);
            });
        }

        public static async void RemoveAsync<TEntity>(this IDbSet<TEntity> modelsProvider, TEntity entity) where TEntity : class
        {
            await Task.Run(() => { modelsProvider.Remove(entity); });
        }
    }
}
