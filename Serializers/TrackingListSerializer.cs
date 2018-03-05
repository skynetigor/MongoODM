using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoORM.Models;
using System.Collections;

namespace MongoORM.Serializers
{
    internal class TrackingICollectionSerializer<T> : SerializerBase<ICollection<T>>
    {
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
