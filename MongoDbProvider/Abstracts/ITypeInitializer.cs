using System;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    internal interface ITypeInitializer
    {
        TypeMetadata GetTypeMetadata<T>();

        TypeMetadata GetTypeMetadata(Type type);

        TypeMetadata RegisterType<T>();

        TypeMetadata RegisterType(Type type);

        bool IsTypeRegistered(Type type);
    }
}
