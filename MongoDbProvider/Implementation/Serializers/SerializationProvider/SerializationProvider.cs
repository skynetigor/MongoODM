using System;
using System.Collections.Generic;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Implementation.TypeMetadataInitializer;
using MongoDB.Bson.Serialization;

namespace DbdocFramework.MongoDbProvider.Implementation.Serializers.SerializationProvider
{
    class SerializationProvider : IBsonSerializationProvider
    {
        private Func<Type, object>[] SerializersFuncs { get; }
        private ITypeInitializer TypeInitializer { get; }

        public SerializationProvider(ITypeInitializer typeInitializer)
        {
            this.TypeInitializer = typeInitializer;
            this.SerializersFuncs = new Func<Type, object>[] {SimpleSerializer, CollectionSerializer};
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            foreach (var serializersFunc in SerializersFuncs)
            {
                object serializer = serializersFunc(type);

                if (serializer != null)
                {
                    return serializer as IBsonSerializer;
                }
            }

            return null;
        }

        private object SimpleSerializer(Type current)
        {
            if (TypeInitializerStatic.IsTypeRegistered(current))
            {
                return Activator.CreateInstance(typeof(ModelsSerializer<>).MakeGenericType(current), new object[] {TypeInitializer});
            }

            return null;
        }

        private object CollectionSerializer(Type current)
        {
            if (current.IsGenericType)
            {
                Type genericDefinition = current.GetGenericTypeDefinition();
                Type genericArgument = current.GetGenericArguments()[0];

                if (TypeInitializerStatic.IsTypeRegistered(genericArgument))
                {
                    if (genericDefinition == typeof(ICollection<>))
                    {
                        return Activator.CreateInstance(
                            typeof(TrackingICollectionSerializer<>).MakeGenericType(genericArgument));
                    }
                    else if (genericDefinition == typeof(IList<>))
                    {
                        return Activator.CreateInstance(
                            typeof(TrackingIListSerializer<>).MakeGenericType(genericArgument));
                    }
                }
            }

            return null;
        }
    }
}
