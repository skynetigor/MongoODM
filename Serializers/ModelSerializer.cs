using System.Collections.Generic;
using MongoORM.Abstracts;
using MongoDB.Bson;
using System.Linq;

namespace MongoORM.Serializers
{
    internal class ModelSerializer<T> : IModelSerializer<BsonDocument>
    {
        private const string MongoIdProperty = "_id";
        private readonly ITypeInitializer typeInitializer;

        public ModelSerializer(ITypeInitializer typeInitializer)
        {
            this.typeInitializer = typeInitializer;
        }

        public BsonDocument Serialize<TEntity>(TEntity entity) where TEntity : class
        {
            var thisTypeModel = this.typeInitializer.InitializeType<TEntity>();

            var entType = thisTypeModel.CurrentType;
            var id = entType.GetProperty(thisTypeModel.IdName).GetValue(entity).ToString();
            var document = new BsonDocument
            {
                { MongoIdProperty, id }
            };

            foreach (var prop in entType.GetProperties().Where(p => p.Name != thisTypeModel.IdName))
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
                        var tmodel = this.typeInitializer.GetTypeModel(prop.PropertyType);
                        var currentProp = prop.PropertyType.GetProperty(tmodel.IdName);
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
