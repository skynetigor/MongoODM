using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbdocFramework.Abstracts;
using DbdocFramework.MongoDbProvider.Extensions;
using DbdocFramework.MongoDbProvider.Helpers;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Bson;

namespace DbdocFramework.MongoDbProvider.Implementation.TypeMetadataInitializer
{
    internal static class TypeInitializerStatic
    {
        const string CollectionPostfix = "s";

        private static Dictionary<Type, TypeMetadata> DictionaryTypeMetadata { get; } =
            new Dictionary<Type, TypeMetadata>();

        public static void RegisterType(Type type)
        {
            if (!DictionaryTypeMetadata.ContainsKey(type))
            {
                var typeModel = Setup(type);
                DictionaryTypeMetadata.Add(type, typeModel);
                InitializeQuery(typeModel);
            }
        }

        public static bool IsTypeRegistered(Type type)
        {
            return DictionaryTypeMetadata.ContainsKey(type);
        }

        public static TypeMetadata GetTypeMetadata(Type type)
        {
            if (!DictionaryTypeMetadata.TryGetValue(type, out var typeModel))
            {
                return null;
            }

            return typeModel;
        }

        private static TypeMetadata Setup(Type type)
        {
            var model = new TypeMetadata {CurrentType = type};

            var attributes = type.GetCustomAttributes<Attribute>().OfType<ITypeMetadata>();

            foreach (var attr in attributes)
            {
                attr.Map(model, type);
            }

            SetProperties(type, model);
            return model;
        }

        private static void SetProperties(Type type, TypeMetadata model)
        {
            if (string.IsNullOrEmpty(model.CollectionName))
            {
                model.CollectionName = type.Name + CollectionPostfix;
            }

            if (model.IdProperty == null)
            {
                model.IdProperty = type.GetProperties().FirstOrDefault(
                    prop => prop.Name.Equals(type.Name + "id", StringComparison.OrdinalIgnoreCase)
                            || prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase));
                return;
            }

            throw new MissingMemberException();
        }

        private static void InitializeQuery(TypeMetadata typeMetadata)
        {
            foreach (var prop in typeMetadata.CurrentType.GetProperties())
            {
                var propType = prop.PropertyType;
                if (propType.IsClass && propType != typeof(string) || propType.IsAbstract)
                {
                    var lookUp = new BsonDocument();
                    if (propType.IsIEnumerableType())
                    {
                        var gerType = propType.GetGenericArguments()[0];

                        var navigationProp = gerType.GetProperties()
                            .FirstOrDefault(p => p.PropertyType == typeMetadata.CurrentType);

                        if (navigationProp != null)
                        {
                            RegisterType(gerType);
                            var currentTypeMetadata = GetTypeMetadata(gerType);
                            lookUp["$lookup"] = new BsonDocument
                            {
                                {"from", currentTypeMetadata.CollectionName},
                                {"localField", "_id"},
                                {"foreignField", navigationProp.GetNavigationPropertyName()},
                                {"as", prop.Name}
                            };
                            typeMetadata.QueryDictionary[prop.Name] = new[] {lookUp};
                        }
                    }
                    else
                    {
                        RegisterType(propType);
                        var tmodel = GetTypeMetadata(propType);
                        lookUp["$lookup"] = new BsonDocument
                        {
                            {"from", tmodel.CollectionName},
                            {"localField", prop.GetNavigationPropertyName()},
                            {"foreignField", "_id"},
                            {"as", prop.Name}
                        };

                        var unwind = new BsonDocument
                        {
                            ["$unwind"] = new BsonDocument
                            {
                                {"path", $"${prop.Name}"},
                                {"preserveNullAndEmptyArrays", true}
                            }
                        };
                        typeMetadata.QueryDictionary[prop.Name] = new[] {lookUp, unwind};
                    }
                }
            }
        }
    }
}
