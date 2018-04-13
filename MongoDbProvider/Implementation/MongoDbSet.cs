using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;
using DbdocFramework.Abstracts;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Helpers;
using DbdocFramework.MongoDbProvider.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DbdocFramework.MongoDbProvider.Implementation
{
    internal class MongoDbSet<TEntity> : IDbSet<TEntity> where TEntity : class, new()
    {
        const string MongoIdProperty = "_id";
        private IMongoDatabase Database { get; }
        private ITypeInitializer TypeInitializer { get; }
        private TypeMetadata CurrentTypeModel { get; }
        private IDbsetContainer DbsetContainer { get; }
        private MethodInfo SetRelationsMethod { get; }
        private ICustomServiceProvider ServiceProvider { get; }

        public MongoDbSet(IMongoDatabase database, IDbsetContainer dbsetContainer,ITypeInitializer typeInitializer, ICustomServiceProvider serviceProvider)
        {
            this.Database = database;
            this.TypeInitializer = typeInitializer;
            this.CurrentTypeModel = TypeInitializer.GetTypeMetadata<TEntity>();
            this.ServiceProvider = serviceProvider;
            this.DbsetContainer = dbsetContainer;
            this.SetRelationsMethod = this.GetType().GetMethod(nameof(this.SetRelations), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void Add(TEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            this.Database.GetCollection<TEntity>(CurrentTypeModel.CollectionName).InsertOne(entity);
            this.UpdateIncludedToCollectionModels(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            var insertableList = new List<TEntity>();
            var packageCount = 100;
            var currentCount = 0;

            void UpdateAction()
            {
                this.Database.GetCollection<TEntity>(CurrentTypeModel.CollectionName).InsertMany(insertableList);

                foreach (var ent in insertableList)
                {
                    this.UpdateIncludedToCollectionModels(ent);
                }
            }

            foreach (TEntity entity in entities)
            {
                insertableList.Add(entity);

                if (currentCount > 0 && currentCount % packageCount == 0)
                {
                    UpdateAction();

                    insertableList.Clear();
                }

                currentCount++;
            }

            UpdateAction();
        }

        public void Update(TEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            var id = CurrentTypeModel.IdProperty.GetValue(entity);
            var filter = new BsonDocument
            {
                { MongoIdProperty, id.ToString() }
            };
            this.Database.GetCollection<TEntity>(CurrentTypeModel.CollectionName).ReplaceOne(filter, entity);
            this.UpdateIncludedToCollectionModels(entity);
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
            if (entity != null)
            {
                var filter = new BsonDocument { {"_id", (string)this.CurrentTypeModel.IdProperty.GetValue(entity)} };

                this.Database.GetCollection<BsonDocument>(CurrentTypeModel.CollectionName)
                    .DeleteOne(entity.ToBsonDocument());
            }
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var e in entities)
            {
                this.Remove(e);
            }
        }

        public IIncludableQueryable<TEntity> UseLazyLoading()
        {
            return this.ServiceProvider.GetService<ILazyLoadingIncludableQueryable<TEntity>>();
        }

        public IIncludableQueryable<TEntity> UseEagerLoading()
        {
            return this.ServiceProvider.GetService<IEagerLoadingIncludableQueryable<TEntity>>();
        }

        private void UpdateIncludedToCollectionModels(TEntity entity)
        {
            var trackingListType = typeof(TrackingList<>);
            var trackingProps = this.CurrentTypeModel
                .CurrentType.GetProperties()
                .Where(p =>
                {
                    if (p.PropertyType.IsGenericType)
                    {
                        var genericTypeDefinition = p.PropertyType.GetGenericTypeDefinition();
                        return genericTypeDefinition == typeof(ICollection<>) ||
                               genericTypeDefinition == typeof(IList<>);
                    }

                    return false;
                });

            foreach (var trList in trackingProps)
            {
                var trListGenericArgument = trList.PropertyType.GetGenericArguments()[0];
                var trListInstance = trList.GetValue(entity);

                if (trListInstance != null)
                {
                    var trListInstanceType = trListInstance.GetType();

                    if (trListInstanceType != trackingListType)
                    {
                        trListInstance = TrackingListHelper.CreateNewTrackingList(trListGenericArgument, trListInstance);
                        trList.SetValue(entity, trListInstance);
                    }

                    var actualTypeModel = this.TypeInitializer.GetTypeMetadata(trListGenericArgument);

                    if (actualTypeModel != null)
                    {
                        var currentTypeProps = trListGenericArgument.GetProperties().Where(p => p.PropertyType == this.CurrentTypeModel.CurrentType).ToArray();
                        SetRelationsMethod.MakeGenericMethod(trListGenericArgument).Invoke(this, new[] { entity, trListInstance, currentTypeProps });
                    }
                }
            }
        }

        private void SetRelations<T>(TEntity entity, ITrackingList<T> trackingList, IEnumerable<PropertyInfo> props) where T : class
        {
            var newEntities = new List<T>();
            var updatedEntities = new List<T>();
            var tmodel = this.TypeInitializer.GetTypeMetadata<T>();

            foreach (var ent in trackingList.AddedList)
            {
                foreach (var prop in props)
                {
                    if (ent != null)
                    {
                        prop.SetValue(ent, entity);
                    }
                }

                if (tmodel.IdProperty.GetValue(ent) == null)
                {
                    newEntities.Add(ent);
                    continue;
                }

                updatedEntities.Add(ent);
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

            var modelsProvider = this.DbsetContainer.GetDbSet<T>();
            var updated = updatedEntities.Concat(trackingList.RemovedList).ToArray();

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
