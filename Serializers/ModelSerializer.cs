using System.Collections.Generic;
using MongoODM.Abstracts;
using MongoDB.Bson;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoODM.Extensions;

namespace MongoODM.Serializers
{
    internal class ModelSerializer : IModelSerializer<BsonDocument>
    {
        private const string MongoIdProperty = "_id";
        private readonly ITypeInitializer _typeInitializer;

        public ModelSerializer(ITypeInitializer typeInitializer)
        {
            this._typeInitializer = typeInitializer;
        }

        public BsonDocument Serialize<TEntity>(TEntity entity) where TEntity : class
        {
            var entityTypeModel = this._typeInitializer.InitializeType<TEntity>();

            var id = entityTypeModel.IdProperty.GetValue(entity);
            var document = new BsonDocument
            {
                { MongoIdProperty, (string)id }
            };

            foreach (var prop in entityTypeModel.CurrentType.GetProperties())
            {
                object propertyValue = prop.GetValue(entity);

                if (prop.PropertyType.Name == typeof(ICollection<>).Name ||
                    prop.PropertyType.Name == typeof(IList<>).Name || propertyValue == null)
                {
                    continue;
                }

                if (this._typeInitializer.GetTypeModel(prop.PropertyType) != null)
                {
                    var propTypeModel = this._typeInitializer.GetTypeModel(prop.PropertyType);
                    object idValue = propTypeModel.IdProperty.GetValue(propertyValue);

                    if (idValue != null)
                    {
                        if (prop.PropertyType == typeof(string))
                        {
                            document[prop.Name] = (string)idValue;
                        }
                        else
                        {
                            document[prop.Name + "Id"] = propertyValue.ToBson();
                        }
                    }
                }
                else if (prop != entityTypeModel.IdProperty)
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        document[prop.Name] = (string)propertyValue;
                    }
                    else
                    {
                        document[prop.Name] = propertyValue.ToBson();
                    }
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
