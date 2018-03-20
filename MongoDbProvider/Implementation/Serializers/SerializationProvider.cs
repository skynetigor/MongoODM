using System;
using MongoDB.Bson.Serialization;
using DbdocFramework.Abstracts;
using DbdocFramework.DI.Abstract;
using DbdocFramework.DI.Extensions;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Implementation.Serializers;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.MongoDbProvider.Serializers
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
            else if (currentType.Name == typeof(LazyLoadingResult<>).Name)
            {
                var serializerType = typeof(LazyLoadingSerializer<>).MakeGenericType(typeof(T).GetGenericArguments()[0]);
                return (IBsonSerializer)this.ServiceProvider.CreateInstance(serializerType);
            }

            return null;
        }
    }
}
