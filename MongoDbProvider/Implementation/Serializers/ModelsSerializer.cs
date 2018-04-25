using System;
using System.Reflection;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Extensions;
using DbdocFramework.MongoDbProvider.Helpers;
using DbdocFramework.MongoDbProvider.Implementation.TypeMetadataInitializer;
using DbdocFramework.MongoDbProvider.Models;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DbdocFramework.MongoDbProvider.Implementation.Serializers
{
    class ModelsSerializer<T> : SerializerBase<T>, IBsonDocumentSerializer
    {
        private const string MongoIdProperty = "_id";
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

        public ModelsSerializer() : base()
        {
            this.CurentTypeModel = TypeInitializerStatic.GetTypeMetadata(this.ValueType);
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
                    if (propertyValue != null)
                    {
                        {
                            bsonWriter.WriteName(MongoIdProperty);
                            BsonSerializer.LookupSerializer(entityTypeModel.IdProperty.PropertyType)
                                .Serialize(context, propertyValue);
                        }
                    }
                }
                else if (TypeInitializerStatic.GetTypeMetadata(prop.PropertyType) != null && propertyValue != null)
                {
                    var propTypeModel = TypeInitializerStatic.GetTypeMetadata(prop.PropertyType);
                    var idValue = propTypeModel.IdProperty.GetValue(propertyValue);

                    if (idValue != null)
                    {
                        bsonWriter.WriteName(prop.GetNavigationPropertyName());
                        BsonSerializer.LookupSerializer(propTypeModel.IdProperty.PropertyType)
                            .Serialize(context, idValue);
                    }
                }
                else if ((!prop.PropertyType.IsIEnumerableType() || prop.PropertyType == typeof(string)) && propertyValue != null)
                {
                    bsonWriter.WriteName(prop.Name);
                    BsonSerializer.LookupSerializer(prop.PropertyType).Serialize(context, propertyValue);
                }
            }

            bsonWriter.WriteEndDocument();
        }

        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            var model = Activator.CreateInstance<T>();
            reader.ReadStartDocument();
            while (reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = context.Reader.ReadName();

                var property = name == "_id" ? this.CurentTypeModel.IdProperty : model.GetPropertyIgnoreCase(name);

                if (property != null)
                {
                    var value = BsonSerializer.LookupSerializer(property.PropertyType).Deserialize(context, args);

                    property.SetValue(model, value);
                }
                else
                {
                    reader.SkipValue();
                }
            }

            reader.ReadEndDocument();
           return model;
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
