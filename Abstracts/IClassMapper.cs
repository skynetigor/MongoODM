using System;

namespace MongoORM.Abstracts
{
    public interface IClassMapper
    {
        void MapClass<T>();
        void MapClass(Type type);
    }
}
