using MongoDB.Driver;
using System.Collections.Generic;
using MongoODM.Abstracts;
using MongoODM.Models;
using MongoDB.Bson;
using System.Collections;
using System.Linq;
using MongoODM.Serializers;
using System.Reflection;
using MongoODM.Includables;
using System.Linq.Expressions;
using System;

namespace MongoODM.ItemsSets
{
    internal class MongoDbModelsProvider<TEntity> : IModelsProvider<TEntity> where TEntity : class, new()
    {
        const string MongoIdProperty = "_id";
        private IMongoDatabase database;
        private ITypeInitializer typeInitializer;
        private TypeModel currentTypeModel;
        private IModelSerializer<BsonDocument> serializer;
        private static bool isInitialized = false;
        private MongoDbContext context;
        private IIncludableEnumerable<TEntity> includable;

        public MongoDbModelsProvider(IMongoDatabase database, MongoDbContext context)
        {
            this.database = database;
            this.typeInitializer = new TypeInitializer();
            this.currentTypeModel = typeInitializer.InitializeType<TEntity>();
            this.serializer = new ModelSerializer<BsonDocument>(this.typeInitializer);
            this.context = context;
            this.includable = new IncludableEnumerable<TEntity>(database, typeInitializer);
        }

        public void InitializeQuery()
        {
            if (!isInitialized)
            {
                IClassMapper classMapper = new ClassMapper(this.typeInitializer);
                classMapper.MapClass<TEntity>();
                IQueryInitializer queryInitializer = new QueryInitializer(this.typeInitializer);
                queryInitializer.Initialize<TEntity>();
                isInitialized = true;
            }
        }

        public IEnumerable<TEntity> Include()
        {
            return this.includable.Include();
        }

        public IEnumerable<TEntity> Include(params string[] navigationPropsPath)
        {
            return this.includable.Include(navigationPropsPath);
        }

        public IEnumerable<TEntity> Include(params Expression<Func<TEntity, object>>[] navigationPropsPath)
        {
            return this.includable.Include(navigationPropsPath);
        }

        public void Add(TEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            var id = ObjectId.GenerateNewId().ToString();
            this.currentTypeModel.CurrentType.GetProperty(this.currentTypeModel.IdName).SetValue(entity, id);
            var document = this.serializer.Serialize(entity);
            this.database.GetCollection<BsonDocument>(currentTypeModel.CollectionName).InsertOne(document);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            var list = new List<BsonDocument>();
            var prop = this.currentTypeModel.CurrentType.GetProperty(this.currentTypeModel.IdName);
            foreach (var e in entities)
            {
                var id = ObjectId.GenerateNewId().ToString();
                prop.SetValue(e, id);
                var document = this.serializer.Serialize(e);
                list.Add(document);
            }

            this.database.GetCollection<BsonDocument>(currentTypeModel.CollectionName).InsertMany(list);
        }

        public void Update(TEntity entity)
        {
            if (entity == null)
            {
                return;
            }

             this.UpdateIncludedToCollectionModels(entity);

            var document = this.serializer.Serialize(entity);
            var id = document[MongoIdProperty];
            var doc = new BsonDocument(MongoIdProperty, id);
            this.database.GetCollection<BsonDocument>(currentTypeModel.CollectionName).ReplaceOne(doc, document);
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
            var id = this.currentTypeModel.CurrentType
                .GetProperty(currentTypeModel.IdName)
                .GetValue(entity)
                .ToString();
            var doc = new BsonDocument(MongoIdProperty, id);
            this.database.GetCollection<BsonDocument>(currentTypeModel.CollectionName).DeleteOne(doc);
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
            return this.includable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void UpdateIncludedToCollectionModels(TEntity entity)
        {
            var trackingListType = typeof(TrackingList<>);
            var trackingProps = this.currentTypeModel
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

                    var actualTypeModel = this.typeInitializer.GetTypeModel(trListGerType);

                    if (actualTypeModel != null)
                    {
                        var currentTypeProps = trListGerType.GetProperties().Where(p => p.PropertyType == this.currentTypeModel.CurrentType);
                        var t = this.GetType().GetMethod(nameof(this.SetRelations), BindingFlags.NonPublic | BindingFlags.Instance);
                        var method = t.MakeGenericMethod(trListGerType);
                        method.Invoke(this, new[] { entity, trListInstance, currentTypeProps });
                    }
                }

            }
        }

        private void SetRelations<T>(TEntity entity, TrackingList<T> trackingList, IEnumerable<PropertyInfo> props) where T : class
        {
            var newEntities = new List<T>();
            var updatedEntities = new List<T>();
            var tmodel = this.typeInitializer.GetTypeModel<T>();
            var idProp = typeof(T).GetProperty(tmodel.IdName);

            foreach (var ent in trackingList.AddedList)
            {
                if (idProp.GetValue(ent) == null)
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

            var tModel = this.typeInitializer.GetTypeModel<T>();
            var modelsProvider = this.context.Set<T>();
            var updated = updatedEntities.Concat(trackingList.RemovedList);

            if (updated.Count() > 0)
            {
                modelsProvider.UpdateRange(updated);
            }

            if(newEntities.Count() > 0)
            {
                modelsProvider.AddRange(newEntities);

            }

            trackingList.RemovedList.Clear();
            trackingList.AddedList.Clear();
        }

    }
}
