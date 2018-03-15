using System;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    internal interface IQueryInitializer
    {
        void Initialize<T>();
        void Initialize(Type type);
    }
}
