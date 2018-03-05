using MongoORM.Models;
using System;

namespace MongoORM.Abstracts
{
    public interface ITypeInitializer
    {
        TypeModel GetTypeModel<T>();

        TypeModel GetTypeModel(Type type);

        TypeModel InitializeType<T>();

        TypeModel InitializeType(Type type);
    }
}
