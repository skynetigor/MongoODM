using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization;
using MongoODM.Abstracts;
using MongoODM.Extensions;

namespace MongoODM.Serializers
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
