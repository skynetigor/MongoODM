using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using DbdocFramework.Abstracts;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.MongoDbProvider.Serializers
{
    class ModelsSerializer<T> : SerializerBase<T>, IBsonDocumentSerializer
    {
        private const string MongoIdProperty = "_id";
        private readonly ITypeInitializer _typeInitializer;
        private BsonClassMapSerializer<T> _serializer;
        private TypeMetadata CurentTypeModel { get; }

        private BsonClassMapSerializer<T> Serializer
        {
            get
            {
                if (this._serializer == null)
                {
                    var classMap = BsonClassMap.LookupClassMap(this.ValueType);
                    this._serializer = new BsonClassMapSerializer<T>(classMap);
                }

                return this._serializer;
            }
        }

        public ModelsSerializer(ITypeInitializer typeInitializer) : base()
        {
            this._typeInitializer = typeInitializer;
            this.CurentTypeModel = this._typeInitializer.GetTypeMetadata(this.ValueType);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
                return;
            }

            var entityTypeModel = this.CurentTypeModel;

            bsonWriter.WriteStartDocument();

            foreach (var prop in entityTypeModel.CurrentType.GetProperties())
            {
                object propertyValue = prop.GetValue(value);

                if (prop == entityTypeModel.IdProperty)
                {
                    if (propertyValue == null)
                    {
                        var id = this.GetId(entityTypeModel.IdProperty);

                        if (id != null)
                        {
                            entityTypeModel.IdProperty.SetValue(value, id);
                            bsonWriter.WriteName(MongoIdProperty);
                            BsonSerializer.LookupSerializer(entityTypeModel.IdProperty.PropertyType)
                                .Serialize(context, id);
                        }
                    }
                }
                else if (prop.PropertyType.Name == typeof(ICollection<>).Name ||
                    prop.PropertyType.Name == typeof(IList<>).Name || propertyValue == null)
                {
                    continue;
                }
                else if (this._typeInitializer.GetTypeMetadata(prop.PropertyType) != null)
                {
                    var propTypeModel = this._typeInitializer.GetTypeMetadata(prop.PropertyType);
                    var idValue = propTypeModel.IdProperty.GetValue(propertyValue);

                    if (idValue != null)
                    {
                        bsonWriter.WriteName(prop.PropertyType.Name + "Id");
                        BsonSerializer.LookupSerializer(propTypeModel.IdProperty.PropertyType)
                            .Serialize(context, idValue);
                    }
                }
                else
                {
                    bsonWriter.WriteName(prop.Name);
                    BsonSerializer.LookupSerializer(prop.PropertyType).Serialize(context, propertyValue);
                }

            }

            bsonWriter.WriteEndDocument();
        }

        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return this.Serializer.Deserialize(context, args);
        }

        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            foreach (var memberMap in this.ValueType.GetProperties())
            {
                if (memberMap.Name == memberName)
                {
                    if (memberMap == this.CurentTypeModel.IdProperty)
                    {
                        memberName = MongoIdProperty;
                    }

                    var serializer = BsonSerializer.LookupSerializer(memberMap.PropertyType);
                    serializationInfo = new BsonSerializationInfo(memberName, serializer, serializer.ValueType);
                    return true;
                }
            }

            serializationInfo = null;
            return false;
        }

        private object GetId(PropertyInfo idProperty)
        {
            if (idProperty.PropertyType == typeof(string))
            {
                return ObjectId.GenerateNewId().ToString();
            }
            else if (idProperty.PropertyType == typeof(ObjectId))
            {
                return ObjectId.GenerateNewId();
            }

            return null;
        }
    }
}
