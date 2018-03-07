using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoODM.Abstracts;
using MongoODM.Extensions;
using MongoODM.Models;

namespace MongoODM.Serializers
{
    class ModelsSerializer<T> : SerializerBase<T>, IBsonDocumentSerializer
    {
        private const string MongoIdProperty = "_id";
        private readonly ITypeInitializer _typeInitializer;
        private readonly BsonClassMapSerializer<T> serializer;

        public ModelsSerializer(ITypeInitializer typeInitializer) : base()
        {
            this._typeInitializer = typeInitializer;
            this.serializer = new BsonClassMapSerializer<T>(BsonClassMap.LookupClassMap(typeof(T)));
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            var entityTypeModel = this._typeInitializer.InitializeType<T>();
            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            var id = this.GetId(entityTypeModel.IdProperty, value);

            if (id != null)
            {
                bsonWriter.WriteName(MongoIdProperty);
                BsonSerializer.LookupSerializer(entityTypeModel.IdProperty.PropertyType).Serialize(context, id);
            }

            foreach (var prop in entityTypeModel.CurrentType.GetProperties())
            {
                object propertyValue = prop.GetValue(value);

                if (prop.PropertyType.Name == typeof(ICollection<>).Name ||
                    prop.PropertyType.Name == typeof(IList<>).Name || propertyValue == null)
                {
                    continue;
                }

                if (this._typeInitializer.GetTypeModel(prop.PropertyType) != null)
                {
                    var propTypeModel = this._typeInitializer.GetTypeModel(prop.PropertyType);
                    var idValue = this.GetId(propTypeModel.IdProperty, propertyValue);

                    if (idValue != null)
                    {
                        bsonWriter.WriteName(prop.PropertyType.Name + "Id");
                        BsonSerializer.LookupSerializer(propTypeModel.IdProperty.PropertyType).Serialize(context, idValue);
                    }
                }
                else if (prop != entityTypeModel.IdProperty)
                {
                    bsonWriter.WriteName(prop.Name);
                    BsonSerializer.LookupSerializer(prop.PropertyType).Serialize(context, propertyValue);
                }

                bsonWriter.WriteEndDocument();
            }
        }

        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return this.serializer.Deserialize(context, args);
        }

        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            var a = BsonSerializer.LookupSerializer<BsonDocument>();
            var memberType = (typeof(T)).GetProperty(memberName).PropertyType;
            serializationInfo = new BsonSerializationInfo(
                memberName,
                BsonValueSerializer.Instance,
                memberType);
            return true;
        }

        private object GetId(PropertyInfo idProperty, object obj)
        {
            object idValue = idProperty.GetValue(obj);

            if (idValue == null)
            {
                if (idProperty.PropertyType == typeof(string))
                {
                    return ObjectId.GenerateNewId().ToString();
                }
                else if (idProperty.PropertyType == typeof(ObjectId))
                {
                    return ObjectId.GenerateNewId();
                }
            }

            return null;
        }
    }
}
