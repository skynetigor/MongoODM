using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Helpers;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DbdocFramework.MongoDbProvider.Implementation.State.Savers
{
    internal class UpdatedDataChangesSaver<TEntity> : AbstractChangesSaver<TEntity>
    {
        private static MethodInfo SetRelationsMethod { get; }
        private IDbsetContainer DbsetContainer { get; }

        static UpdatedDataChangesSaver()
        {
            SetRelationsMethod = typeof(UpdatedDataChangesSaver<TEntity>).GetMethod(nameof(SetRelations), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public UpdatedDataChangesSaver(IMongoDatabase database, ITypeInitializer typeInitializer, IDbsetContainer dbsetContainer) : base(database,
            typeInitializer)
        {
            DbsetContainer = dbsetContainer;
        }

        protected override void SaveChanges(IEnumerable<TEntity> dataForSaving)
        {
            dataForSaving.ForEach(entity =>
            {
                var id = CurrentTypeModel.IdProperty.GetValue(entity);
                var filter = new BsonDocument
                {
                    { "_id", id.ToString() }
                };

                this.Database.GetCollection<TEntity>(CurrentTypeModel.CollectionName).FindOneAndReplace(filter, entity);
            });
        }

        protected override void BeforeAddData(TEntity entity)
        {
            UpdateIncludedToCollectionModels(entity);
        }

        protected override void BeforeAddRangeData(IEnumerable<TEntity> entities)
        {
            entities.ForEach(UpdateIncludedToCollectionModels);
        }

        private void UpdateIncludedToCollectionModels(TEntity entity)
        {
            SetId(entity);

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

        private void SetId(TEntity entity)
        {
            var idProperty = CurrentTypeModel.IdProperty;
            object idValue = null;

            if (idProperty.PropertyType == typeof(string))
            {
                idValue = ObjectId.GenerateNewId().ToString();
            }
            else if (idProperty.PropertyType == typeof(ObjectId))
            {
                idValue = ObjectId.GenerateNewId();
            }

            idProperty.SetValue(entity, idValue);
        }
    }
}
