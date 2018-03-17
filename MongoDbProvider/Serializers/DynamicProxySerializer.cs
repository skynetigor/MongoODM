using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DbdocFramework.MongoDbProvider.Serializers
{
    class DynamicProxySerializer<T> : SerializerBase<LazyLoadingContainer<T>> where T: class 
    {
        private IMongoDbLazyLoadingInterceptor Interceptor { get; }
        public DynamicProxySerializer(IMongoDbLazyLoadingInterceptor interceptor)
        {
            Interceptor = interceptor;
        }

        public override LazyLoadingContainer<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartDocument();
            context.Reader.ReadName();
            args.NominalType = typeof(T);
            var target = BsonSerializer.LookupSerializer<T>().Deserialize(context, args);
            var proxy = this.Interceptor.CreateProxy<T>(target);

            foreach (var propertyInfo in target.GetType().GetProperties())
            {
                propertyInfo.SetValue(proxy, propertyInfo.GetValue(target));
            }

            context.Reader.ReadEndDocument();
            return new LazyLoadingContainer<T>(proxy, target);
        }
    }
}
