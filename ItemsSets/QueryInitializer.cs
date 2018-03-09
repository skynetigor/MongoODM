using System;
using MongoDB.Bson;
using MongoODM.Abstracts;
using System.Collections.Generic;
using System.Reflection;

namespace MongoODM.ItemsSets
{
    internal class QueryInitializer : IQueryInitializer
    {
        public ITypeInitializer TypeInitializer { get; }

        public QueryInitializer(ITypeInitializer typeInitializer)
        {
            this.TypeInitializer = typeInitializer;
        }

        public void Initialize<T>()
        {
            this.Initialize(typeof(T));
        }

        public void Initialize(Type type)
        {
            var typeMetadata = this.TypeInitializer.GetTypeMetadata(type);

            foreach (var prop in type.GetProperties())
            {
                var propType = prop.PropertyType;
                if (propType.IsClass && propType != typeof(string) || propType.IsInterface)
                {
                    var lookUp = new BsonDocument();
                    if (propType.Name == typeof(ICollection<>).Name || propType.Name == typeof(IEnumerable<>).Name)
                    {
                        var gerType = propType.GetGenericArguments()[0];
                        var currentTypeMetadata = this.TypeInitializer.GetTypeMetadata(gerType);
                        lookUp["$lookup"] = new BsonDocument
                        {
                           { "from", currentTypeMetadata.CollectionName },
                           { "localField", "_id" },
                           { "foreignField", type.Name + "Id"},
                           {"as", prop.Name }
                        };
                        typeMetadata.QueryDictionary[prop.Name] = new[] { lookUp };
                    }
                    else
                    {
                        var tmodel = this.TypeInitializer.GetTypeMetadata(propType);
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
                        typeMetadata.QueryDictionary[prop.Name] = new[] { lookUp, unwind };
                    }
                }
            }
        }
    }
}
