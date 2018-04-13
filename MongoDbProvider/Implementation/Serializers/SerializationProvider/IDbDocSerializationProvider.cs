using System;

namespace DbdocFramework.MongoDbProvider.Implementation.Serializers.SerializationProvider
{
    internal interface IDbDocSerializationProvider
    {
        object TryGetSerializer(Type type);
    }
}
