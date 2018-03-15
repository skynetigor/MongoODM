using System;
using MongoDB.Bson.Serialization;
using DbdocFramework.Abstracts;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;

namespace DbdocFramework.MongoDbProvider.Serializers
{
    class SerializationProvider : IBsonSerializationProvider
    {
        private ITypeInitializer TypeInitializer { get; }

        public SerializationProvider(ITypeInitializer typeInitializer)
        {
            this.TypeInitializer = typeInitializer;
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            return (IBsonSerializer)this.GetPrivateMethod(nameof(GetSerializer)).MakeGenericMethod(type).Invoke(this, null);
        }

        private IBsonSerializer GetSerializer<T>()
        {
            if (this.TypeInitializer.GetTypeMetadata<T>() != null)
            {
                return new ModelsSerializer<T>(this.TypeInitializer);
            }

            return null;
        }
    }
}
