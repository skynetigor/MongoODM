//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MongoDB.Bson;
//using MongoDB.Bson.IO;
//using MongoDB.Bson.Serialization;
//using MongoDB.Bson.Serialization.Serializers;
//using MongoODM.Abstracts;
//using MongoODM.Models;

//namespace MongoODM.Serializers
//{
//    class ModelsSerializerM<T> : SerializerBase<T>
//    {
//        private const string MongoIdProperty = "_id";
//        private readonly ITypeInitializer _typeInitializer;

//        public ModelsSerializerM(ITypeInitializer typeInitializer) : base()
//        {
//            this._typeInitializer = typeInitializer;
//        }

//        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
//        {
//            var bsonWriter = context.Writer;

//            if (value == null)
//            {
//                bsonWriter.WriteNull();
//            }
//            else
//            {
//                bsonWriter.WriteStartDocument();
//                T entity = value;
//                var thisTypeModel = this._typeInitializer.InitializeType<T>();

//                this.WriteId(context, args, value);

//                var properties = value.GetType().GetProperties()
//                    .Where(p => this._typeInitializer.GetTypeModel(p.PropertyType) != null);

//                foreach (var prop in properties)
//                {
//                    var currentValue = prop.GetValue(entity);
//                    bsonWriter.
//                    document.Remove(prop.Name);
//                    document[prop.Name + "Id"] = "null";
//                    if (currentValue != null)
//                    {
//                        var tmodel = this._typeInitializer.GetTypeModel(prop.PropertyType);
//                        var val = thisTypeModel.IdProperty.GetValue(currentValue);

//                        if (val != null)
//                        {
//                            document[prop.Name + "Id"] = val.ToString();
//                        }
//                    }
//                }

//                bsonWriter.WriteEndDocument();
//            }
//        }

//        protected virtual void WriteId(BsonSerializationContext context, BsonSerializationArgs args, T value)
//        {
//            IBsonWriter bsonWriter = context.Writer;
//            TypeModel current = _typeInitializer.GetTypeModel(typeof(T));
//            var idValue = current.IdProperty.GetValue(value);

//            if (idValue != null)
//            {
//                var id = BsonSerializer.Serialize(bsonWriter)
//                bsonWriter.
//            }
//        }
//    }
//}
