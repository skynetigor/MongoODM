using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoODM.Abstracts;

namespace MongoODM.Serializers
{
    class NewModelSerializer : IModelSerializer<BsonDocument>
    {
        private const string MongoIdProperty = "_id";
        private readonly ITypeInitializer _typeInitializer;

        public NewModelSerializer(ITypeInitializer typeInitializer)
        {
            this._typeInitializer = typeInitializer;
        }

        public BsonDocument Serialize<TEntity>(TEntity entity) where TEntity : class
        {
            var thisTypeModel = this._typeInitializer.InitializeType<TEntity>();

            var entType = thisTypeModel.CurrentType;
            var id = thisTypeModel.IdProperty.GetValue(entity).ToString();
            var document = new BsonDocument
            {
                {MongoIdProperty, id}
            };

            foreach (var prop in entType.GetProperties().Where(p => p.Name != thisTypeModel.IdProperty.Name))
            {
                if (prop.PropertyType.Name == typeof(ICollection<>).Name
                    || prop.PropertyType.Name == typeof(IEnumerable<>).Name)
                {
                    continue;
                }

                else if (prop.PropertyType.IsClass
                         && prop.PropertyType != typeof(string))
                {
                    var currentValue = prop.GetValue(entity);
                    document.Remove(prop.Name);
                    document[prop.Name + "Id"] = "null";
                    if (currentValue != null)
                    {
                        var tmodel = this._typeInitializer.GetTypeModel(prop.PropertyType);
                        var currentProp = tmodel.IdProperty;
                        var val = currentProp.GetValue(currentValue);

                        if (val != null)
                        {
                            document[prop.Name + "Id"] = val.ToString();
                        }
                    }
                }
                else
                {
                    document[prop.Name] = prop.GetValue(entity).ToString();
                }
            }

            return document;
        }

        public IEnumerable<BsonDocument> Serialize<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            return entities.Select(this.Serialize<TEntity>);
        }
    }
}

