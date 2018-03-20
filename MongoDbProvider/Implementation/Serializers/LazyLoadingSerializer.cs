using System.Collections.Generic;
using System.Linq;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DbdocFramework.MongoDbProvider.Implementation.Serializers
{
    class LazyLoadingSerializer<T> : SerializerBase<LazyLoadingResult<T>> where T : class
    {
        private IMongoDbLazyLoadingInterceptor Interceptor { get; }
        public LazyLoadingSerializer(IMongoDbLazyLoadingInterceptor interceptor)
        {
            Interceptor = interceptor;
        }

        public override LazyLoadingResult<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartDocument();

            IEnumerable<T> result;
            var bsonType = context.Reader.ReadBsonType();
            context.Reader.ReadName();

            if (bsonType == BsonType.Document)
            {
                args.NominalType = typeof(T);
                result = new T[] { BsonSerializer.LookupSerializer<T>().Deserialize(context, args) };
            }
            else
            {
                result = Enumerable.Empty<T>();
            }

            context.Reader.ReadEndDocument();

            result = this.Interceptor.CreateProxies(result);

            return new LazyLoadingResult<T>(result);
        }
    }
}
