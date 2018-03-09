using System;

namespace MongoODM.Abstracts
{
    public interface IQueryInitializer
    {
        void Initialize<T>();
        void Initialize(Type type);
    }
}
