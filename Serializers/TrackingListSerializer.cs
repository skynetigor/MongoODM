using MongoDB.Bson.Serialization.Serializers;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoODM.Models;

namespace MongoODM.Serializers
{
    internal class TrackingICollectionSerializer<T> : SerializerBase<ICollection<T>>
    {
        public TrackingICollectionSerializer()
        {
            
        }

        public override ICollection<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            ArraySerializer<T> ser = new ArraySerializer<T>();
            var arr = ser.Deserialize(context, args);
            return new TrackingList<T>(arr);
        }
    }

    internal class TrackingIListSerializer<T> : SerializerBase<IList<T>>
    {
        public override IList<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            ArraySerializer<T> ser = new ArraySerializer<T>();
            var arr = ser.Deserialize(context, args);
            return new TrackingList<T>(arr);
        }
    }
}
