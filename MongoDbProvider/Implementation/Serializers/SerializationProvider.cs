using System;
using DbdocFramework.DI.Abstract;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;
using DbdocFramework.MongoDbProvider.Serializers;
using MongoDB.Bson.Serialization;

namespace DbdocFramework.MongoDbProvider.Implementation.Serializers
{
    class SerializationProvider : IBsonSerializationProvider
    {
        private ITypeInitializer TypeInitializer { get; }
        private ICustomServiceProvider ServiceProvider { get; }

        public SerializationProvider(ITypeInitializer typeInitializer, ICustomServiceProvider serviceProvider)
        {
            this.TypeInitializer = typeInitializer;
            this.ServiceProvider = serviceProvider;
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            return (IBsonSerializer)this.GetPrivateMethod(nameof(GetSerializer)).MakeGenericMethod(type).Invoke(this, null);
        }

        private IBsonSerializer GetSerializer<T>()
        {
            var currentType = typeof(T);

            if (this.TypeInitializer.GetTypeMetadata<T>() != null)
            {
                return new ModelsSerializer<T>(this.TypeInitializer);
            }

            return null;
        }
    }
}
