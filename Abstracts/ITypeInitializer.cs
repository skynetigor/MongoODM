using MongoODM.Models;
using System;

namespace MongoODM.Abstracts
{
    public interface ITypeInitializer
    {
        TypeMetadata GetTypeMetadata<T>();

        TypeMetadata GetTypeMetadata(Type type);

        TypeMetadata RegisterType<T>();

        TypeMetadata RegisterType(Type type);
    }
}
