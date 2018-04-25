using System.Collections.Generic;
using DbdocFramework.Abstracts;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using Microsoft.Extensions.DependencyInjection;

namespace DbdocFramework.MongoDbProvider.Implementation
{
    class MongoDbSetSaveChanges<TEntity> : IDbSet<TEntity> where TEntity : class
    {
        private ICustomServiceProvider ServiceProvider { get; }
        private IState<TEntity> StateManager { get; }

        public MongoDbSetSaveChanges(ICustomServiceProvider serviceProvider, IStateManager stateManager)
        {
            this.ServiceProvider = serviceProvider;
            this.StateManager = stateManager.GetOrCreateState<TEntity>();
        }

        public void Add(TEntity entity)
        {
            StateManager.Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            StateManager.AddRange(entities);
        }

        public void Remove(TEntity entity)
        {
            StateManager.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            StateManager.RemoveRange(entities);
        }

        public void Update(TEntity entity)
        {
            StateManager.Update(entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            StateManager.UpdateRange(entities);
        }

        public IIncludableQueryable<TEntity> UseEagerLoading()
        {
            return this.ServiceProvider.GetService<IEagerLoadingIncludableQueryable<TEntity>>();
        }

        public IIncludableQueryable<TEntity> UseLazyLoading()
        {
            return this.ServiceProvider.GetService<ILazyLoadingIncludableQueryable<TEntity>>();
        }
    }
}
