using MongoDB.Driver;
using System.Collections.Generic;
using MongoODM.Abstracts;
using MongoODM.Models;
using MongoDB.Bson;
using System.Collections;
using System.Linq;
using System.Reflection;
using MongoODM.Includables;
using System.Linq.Expressions;
using System;
using MongoODM.DI.Abstract;
using MongoODM.DI.Extensions;

namespace MongoODM.ItemsSets
{
    internal class MongoDbModelsProvider<TEntity> : IModelsProvider<TEntity> where TEntity : class, new()
    {
        const string MongoIdProperty = "_id";
        private IMongoDatabase Database { get; }
        private ITypeInitializer TypeInitializer { get; }
        private TypeMetadata CurrentTypeModel { get; }
        private MongoDbContext Context { get; }
        private IIncludableEnumerable<TEntity> Includable { get; }
        private MethodInfo SetRelationsMethod { get; }

        public MongoDbModelsProvider(IMongoDatabase database, MongoDbContext context, ITypeInitializer typeInitializer, ICustomServiceProvider type)
        {
            this.Database = database;
            this.TypeInitializer = typeInitializer;
            this.CurrentTypeModel = TypeInitializer.RegisterType<TEntity>();

            if (context.DropCollectionsWhenContextCreating)
            {
                database.DropCollection(CurrentTypeModel.CollectionName);
            }

            this.Context = context;
            this.Includable = type.CreateInstance<IncludableEnumerable<TEntity>>();
            this.SetRelationsMethod = this.GetType().GetMethod(nameof(this.SetRelations), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public IQueryable<TEntity> Include()
        {
            return this.Includable.Include();
        }

        public IQueryable<TEntity> Include(params string[] navigationPropsPath)
        {
            return this.Includable.Include(navigationPropsPath);
        }

        public IQueryable<TEntity> AsQueryable()
        {
            return this.Includable.AsQueryable();
        }

        public IQueryable<TEntity> Include(params Expression<Func<TEntity, object>>[] navigationPropsPath)
        {
            return this.Includable.Include(navigationPropsPath);
        }

        public void Add(TEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            this.Database.GetCollection<TEntity>(CurrentTypeModel.CollectionName).InsertOne(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            var insertableList = new List<TEntity>();
            var packageCount = 100;
            var currentCount = 0;

            foreach (TEntity entity in entities)
            {
                insertableList.Add(entity);

                if (currentCount > 0 && currentCount % packageCount == 0)
                {
                    this.Database.GetCollection<TEntity>(CurrentTypeModel.CollectionName).InsertMany(insertableList);
                    insertableList.Clear();
                }

                currentCount++;
            }

            this.Database.GetCollection<TEntity>(CurrentTypeModel.CollectionName).InsertMany(insertableList);
        }

        public void Update(TEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            this.UpdateIncludedToCollectionModels(entity);

            var id = CurrentTypeModel.IdProperty.GetValue(entity);
            var filter = new BsonDocument
            {
                { MongoIdProperty, id.ToString() }
            };
            this.Database.GetCollection<TEntity>(CurrentTypeModel.CollectionName).ReplaceOne(filter, entity);
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            foreach (var e in entities)
            {
                this.Update(e);
            }
        }

        public void Remove(TEntity entity)
        {
            this.Database.GetCollection<BsonDocument>(CurrentTypeModel.CollectionName).DeleteOne(entity.ToBsonDocument());
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var e in entities)
            {
                this.Remove(e);
            }
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return this.Includable.AsQueryable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void UpdateIncludedToCollectionModels(TEntity entity)
        {
            var trackingListType = typeof(TrackingList<>);
            var trackingProps = this.CurrentTypeModel
                .CurrentType.GetProperties()
                .Where(p => (p.PropertyType.IsClass || p.PropertyType.IsInterface) && p.PropertyType != typeof(string));

            foreach (var trList in trackingProps)
            {
                var trListGerType = trList.PropertyType.GetGenericArguments().FirstOrDefault();
                var trListInstance = trList.GetValue(entity);

                if (trListInstance != null)
                {
                    var st = trListInstance.GetType().Name;

                    if (st != trackingListType.Name)
                    {
                        continue;
                    }

                    var actualTypeModel = this.TypeInitializer.GetTypeMetadata(trListGerType);

                    if (actualTypeModel != null)
                    {
                        var currentTypeProps = trListGerType.GetProperties().Where(p => p.PropertyType == this.CurrentTypeModel.CurrentType);
                        this.SetRelationsMethod.MakeGenericMethod(trListGerType).Invoke(this, new[] { entity, trListInstance, currentTypeProps });
                    }
                }
            }
        }

        private void SetRelations<T>(TEntity entity, TrackingList<T> trackingList, IEnumerable<PropertyInfo> props) where T : class
        {
            var newEntities = new List<T>();
            var updatedEntities = new List<T>();
            var tmodel = this.TypeInitializer.GetTypeMetadata<T>();

            foreach (var ent in trackingList.AddedList)
            {
                if (this.CurrentTypeModel.IdProperty.GetValue(ent) == null)
                {
                    newEntities.Add(ent);
                    continue;
                }

                updatedEntities.Add(ent);
            }

            foreach (var ent in trackingList.AddedList)
            {
                foreach (var prop in props)
                {
                    if (ent != null)
                    {
                        prop.SetValue(ent, entity);
                    }
                }
            }

            foreach (var ent in trackingList.RemovedList)
            {
                foreach (var prop in props)
                {
                    if (ent != null)
                    {
                        prop.SetValue(ent, null);
                    }
                }
            }

            var tModel = this.TypeInitializer.GetTypeMetadata<T>();
            var modelsProvider = this.Context.Set<T>();
            var updated = updatedEntities.Concat(trackingList.RemovedList);

            if (updated.Any())
            {
                modelsProvider.UpdateRange(updated);
            }

            if (newEntities.Any())
            {
                modelsProvider.AddRange(newEntities);
            }

            trackingList.RemovedList.Clear();
            trackingList.AddedList.Clear();
        }
    }
}
