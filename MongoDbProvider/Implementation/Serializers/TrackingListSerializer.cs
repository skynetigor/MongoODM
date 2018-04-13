using System.Collections.Generic;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DbdocFramework.MongoDbProvider.Implementation.Serializers
{
    internal class TrackingICollectionSerializer<T> : SerializerBase<ICollection<T>>
    {
        public override ICollection<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var array = BsonSerializer.LookupSerializer<T[]>().Deserialize(context, args);
            return TrackingList<T>.CreateExistingTrackingList(array);
        }
    }

    internal class TrackingIListSerializer<T> : SerializerBase<IList<T>>
    {
        public override IList<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var array = BsonSerializer.LookupSerializer<T[]>().Deserialize(context, args);
            return TrackingList<T>.CreateExistingTrackingList(array);
        }
    }
}
