using MongoDB.Bson;
using MongoODM.Abstracts;
using System.Collections.Generic;

namespace MongoODM.ItemsSets
{
    internal class QueryInitializer : IQueryInitializer
    {
        public ITypeInitializer typeInitializer;

        public QueryInitializer(ITypeInitializer typeInitializer)
        {
            this.typeInitializer = typeInitializer;
        }

        public void Initialize<T>()
        {
            var typeModelThis = this.typeInitializer.GetTypeModel<T>();
            //var docList = new List<BsonDocument>();
            var thisType = typeof(T);
            foreach (var prop in thisType.GetProperties())
            {
                var propType = prop.PropertyType;
                if (propType.IsClass && propType != typeof(string) || propType.IsInterface)
                {
                    var lookUp = new BsonDocument();
                    if (propType.Name == typeof(ICollection<>).Name || propType.Name == typeof(IEnumerable<>).Name)
                    {
                        var gerType = propType.GetGenericArguments()[0];
                        var tmodel = this.typeInitializer.GetTypeModel(gerType);
                        lookUp["$lookup"] = new BsonDocument
                        {
                           { "from", tmodel.CollectionName },
                           { "localField", "_id" },
                           { "foreignField", thisType.Name + "Id"},
                           {"as", prop.Name }
                        };
                        typeModelThis.QueryDictionary[prop.Name] = new[] { lookUp };
                    }
                    else
                    {
                        var tmodel = this.typeInitializer.GetTypeModel(propType);
                        lookUp["$lookup"] = new BsonDocument
                        {
                           { "from", tmodel.CollectionName },
                           { "localField", propType.Name + "Id" },
                           { "foreignField", "_id"},
                           {"as", prop.Name }
                        };

                        var unwind = new BsonDocument();
                        unwind["$unwind"] = new BsonDocument
                        {
                            { "path", $"${prop.Name}" },
                            { "preserveNullAndEmptyArrays", true }
                        };
                        typeModelThis.QueryDictionary[prop.Name] = new[] { lookUp, unwind };

                    }
                }
            }
        }
    }
}
