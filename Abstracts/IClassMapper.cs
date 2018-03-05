using System;

namespace MongoODM.Abstracts
{
    public interface IClassMapper
    {
        void MapClass<T>();
        void MapClass(Type type);
    }
}
