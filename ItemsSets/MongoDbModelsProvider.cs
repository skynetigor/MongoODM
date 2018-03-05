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
        private readonly IMongoDatabase _database;
        private readonly ITypeInitializer _typeInitializer;
        private readonly TypeModel _currentTypeModel;
        private readonly IModelSerializer<BsonDocument> _serializer;
        private static bool _isInitialized = false;
        private readonly MongoDbContext _context;
        private readonly IIncludableEnumerable<TEntity> _includable;
        private readonly MethodInfo _setRelationsMethod;

        public MongoDbModelsProvider(IMongoDatabase database, MongoDbContext context)
        {
            this._database = database;
            this._typeInitializer = new TypeInitializer();
            this._currentTypeModel = _typeInitializer.InitializeType<TEntity>();
            this._serializer = new ModelSerializer(this._typeInitializer);
            this._context = context;
            this._includable = new IncludableEnumerable<TEntity>(database, _typeInitializer);
            this._setRelationsMethod = this.GetType().GetMethod(nameof(this.SetRelations), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void InitializeQuery()
        {
            if (!_isInitialized)
            {
                IClassMapper classMapper = new ClassMapper(this._typeInitializer);
                classMapper.MapClass<TEntity>();
                IQueryInitializer queryInitializer = new QueryInitializer(this._typeInitializer);
                queryInitializer.Initialize<TEntity>();
                _isInitialized = true;
            }
        }

        public IEnumerable<TEntity> Include()
        {
            return this._includable.Include();
        }

        public IEnumerable<TEntity> Include(params string[] navigationPropsPath)
        {
            return this._includable.Include(navigationPropsPath);
        }

        public IEnumerable<TEntity> Include(params Expression<Func<TEntity, object>>[] navigationPropsPath)
        {
            return this._includable.Include(navigationPropsPath);
        }

        public void Add(TEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            var id = ObjectId.GenerateNewId().ToString();
            this._currentTypeModel.CurrentType.GetProperty(this._currentTypeModel.IdName).SetValue(entity, id);
            var document = this._serializer.Serialize(entity);
            this._database.GetCollection<BsonDocument>(_currentTypeModel.CollectionName).InsertOne(document);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            var prop = this._currentTypeModel.CurrentType.GetProperty(this._currentTypeModel.IdName);

            IEnumerable<BsonDocument> insertableSequance = entities.Select(ent =>
             {
                 var id = ObjectId.GenerateNewId().ToString();
                 prop.SetValue(ent, id);
                 return this._serializer.Serialize(ent);
             });

            entities = null;

            this._database.GetCollection<BsonDocument>(_currentTypeModel.CollectionName).InsertMany(insertableSequance);
        }

        public void Update(TEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            this.UpdateIncludedToCollectionModels(entity);

            var document = this._serializer.Serialize(entity);
            var id = document[MongoIdProperty];
            var filter = new BsonDocument(MongoIdProperty, id);
            this._database.GetCollection<BsonDocument>(_currentTypeModel.CollectionName).ReplaceOne(filter, document);
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
            var id = this._currentTypeModel.CurrentType
                .GetProperty(_currentTypeModel.IdName)
                .GetValue(entity)
                .ToString();
            var doc = new BsonDocument(MongoIdProperty, id);
            this._database.GetCollection<BsonDocument>(_currentTypeModel.CollectionName).DeleteOne(doc);
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
            return this._includable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void UpdateIncludedToCollectionModels(TEntity entity)
        {
            var trackingListType = typeof(TrackingList<>);
            var trackingProps = this._currentTypeModel
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

                    var actualTypeModel = this._typeInitializer.GetTypeModel(trListGerType);

                    if (actualTypeModel != null)
                    {
                        var currentTypeProps = trListGerType.GetProperties().Where(p => p.PropertyType == this._currentTypeModel.CurrentType);
                        this._setRelationsMethod.MakeGenericMethod(trListGerType).Invoke(this, new[] { entity, trListInstance, currentTypeProps });
                    }
                }
            }
        }

        private void SetRelations<T>(TEntity entity, TrackingList<T> trackingList, IEnumerable<PropertyInfo> props) where T : class
        {
            var newEntities = new List<T>();
            var updatedEntities = new List<T>();
            var tmodel = this._typeInitializer.GetTypeModel<T>();
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

            var tModel = this._typeInitializer.GetTypeModel<T>();
            var modelsProvider = this._context.Set<T>();
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
