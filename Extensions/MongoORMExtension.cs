using System.Threading.Tasks;
using MongoODM.Abstracts;

namespace MongoODM.Extensions
{
    public static class MongoORMExtension
    {
        public static void AddAsync<TEntity>(this IModelsProvider<TEntity> modelsProvider, TEntity entity) where TEntity : class
        {
            modelsProvider.Add(entity);
        }

        public static async void UpdateAsync<TEntity>(this IModelsProvider<TEntity> modelsProvider, TEntity entity) where TEntity : class
        {
            await Task.Run(() =>
            {
                modelsProvider.Update(entity);
            });
        }

        public static async void RemoveAsync<TEntity>(this IModelsProvider<TEntity> modelsProvider, TEntity entity) where TEntity : class
        {
            await Task.Run(() => { modelsProvider.Remove(entity); });
        }
    }
}
