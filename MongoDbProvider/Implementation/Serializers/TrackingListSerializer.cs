using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.MongoDbProvider.Serializers
{
    internal class TrackingICollectionSerializer<T> : SerializerBase<ICollection<T>>
    {
        public TrackingICollectionSerializer()
        {
            
        }

        public override ICollection<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var array = BsonSerializer.LookupSerializer<T[]>().Deserialize(context, args);
            return new TrackingList<T>(array);
        }
    }

    internal class TrackingIListSerializer<T> : SerializerBase<IList<T>>
    {
        public override IList<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var array = BsonSerializer.LookupSerializer<T[]>().Deserialize(context, args);
            return new TrackingList<T>(array);
        }
    }
}
