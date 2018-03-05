using MongoODM.Models;
using System;

namespace MongoODM.Abstracts
{
    public interface ITypeInitializer
    {
        TypeModel GetTypeModel<T>();

        TypeModel GetTypeModel(Type type);

        TypeModel InitializeType<T>();

        TypeModel InitializeType(Type type);
    }
}
