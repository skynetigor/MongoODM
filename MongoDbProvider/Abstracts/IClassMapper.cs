using System;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    internal interface IClassMapper
    {
        void MapClass<T>();

        void MapClass(Type type);
    }
}
