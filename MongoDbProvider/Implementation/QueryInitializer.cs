using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Helpers;

namespace DbdocFramework.MongoDbProvider.Implementation
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

                        var navigationProp = gerType.GetProperties()
                            .FirstOrDefault(p => p.PropertyType == type);

                        if (navigationProp == null)
                        {
                            continue;
                        }

                        var currentTypeMetadata = this.TypeInitializer.GetTypeMetadata(gerType);
                        lookUp["$lookup"] = new BsonDocument
                        {
                           { "from", currentTypeMetadata.CollectionName },
                           { "localField", "_id" },
                           { "foreignField", navigationProp.GetNavigationPropertyName()},
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
                           { "localField", prop.GetNavigationPropertyName()},
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
