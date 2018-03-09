using MongoDB.Bson.Serialization;
using MongoODM.Abstracts;
using MongoODM.Models;
using MongoODM.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoODM.Extensions;

namespace MongoODM.ItemsSets
{
    internal class ClassMapper : IClassMapper
    {
        private readonly ITypeInitializer _typeInitializer;
        private MethodInfo _mapClassGenericMethod;
        private IBsonSerializationProvider SerializationProvider;

        public ClassMapper(ITypeInitializer typeInitializer, IBsonSerializationProvider provider)
        {
            this._typeInitializer = typeInitializer;
            this.SerializationProvider = provider;
        }

        public void MapClass<T>()
        {
            var type = typeof(T);

            BsonSerializer.RegisterSerializationProvider(this.SerializationProvider);

            if (!BsonClassMap.IsClassMapRegistered(type))
            {
                BsonClassMap.RegisterClassMap<T>(
                      cm =>
                      {
                          cm.AutoMap();
                          cm.SetIgnoreExtraElements(true);
                          foreach (var prop in type.GetProperties())
                          {
                              if ((prop.PropertyType.Name == typeof(ICollection<>).Name || prop.PropertyType.Name == typeof(IList<>).Name) //Needs to be refactored
                                  && this._typeInitializer.GetTypeMetadata(prop.PropertyType.GetGenericArguments()[0]) != null)
                              {
                                  BsonMemberMap collectionMemberMap = cm.MapProperty(prop.Name);
                                  var genericType = prop.PropertyType.GetGenericArguments()[0];
                                  var trackingListType = typeof(TrackingList<>).MakeGenericType(genericType);
                                  cm.MapProperty(prop.Name).SetDefaultValue(() => Activator.CreateInstance(trackingListType));

                                  if (prop.PropertyType.Name == typeof(ICollection<>).Name)
                                  {
                                      var collectionSerializer =
                                          typeof(TrackingICollectionSerializer<>).MakeGenericType(genericType);
                                      collectionMemberMap.SetSerializer((IBsonSerializer)Activator.CreateInstance(collectionSerializer));
                                  }
                                  else
                                  {
                                      var listSerializer =
                                          typeof(TrackingIListSerializer<>).MakeGenericType(genericType);
                                      collectionMemberMap.SetSerializer((IBsonSerializer)Activator.CreateInstance(listSerializer));
                                  }
                              }
                          }
                      });

                BsonSerializer.RegisterSerializer<T>(new ModelsSerializer<T>(this._typeInitializer));
            }

        }

        public void MapClass(Type type)
        {
            if (this._mapClassGenericMethod == null)
            {
                this._mapClassGenericMethod = this.GetMethods()
                    .FirstOrDefault(m => m.Name == nameof(this.MapClass));
            }

            this._mapClassGenericMethod.MakeGenericMethod(type).Invoke(this, null);
        }

    }
}
