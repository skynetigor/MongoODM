using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;
using DbdocFramework.Abstracts;
using DbdocFramework.DI.Abstract;
using DbdocFramework.DI.Extensions;
using DbdocFramework.Extensions;
using DbdocFramework.MongoDbProvider.Abstracts;
using DbdocFramework.MongoDbProvider.Models;
using DbdocFramework.MongoDbProvider.Serializers;
using MongoDB.Bson.IO;

namespace DbdocFramework.MongoDbProvider.Implementation
{
    internal class ClassMapper : IClassMapper
    {
        private MethodInfo _mapClassGenericMethod;
        private ITypeInitializer TypeInitializer { get; }
        private IBsonSerializationProvider SerializationProvider { get; }
        private ICustomServiceProvider ServiceProvider { get; }

        public ClassMapper(ITypeInitializer typeInitializer, IBsonSerializationProvider provider, ICustomServiceProvider serviceProvider)
        {
            this.TypeInitializer = typeInitializer;
            this.SerializationProvider = provider;
            this.ServiceProvider = serviceProvider;
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
                                  && this.TypeInitializer.GetTypeMetadata(prop.PropertyType.GetGenericArguments()[0]) != null)
                              {
                                  BsonMemberMap collectionMemberMap = cm.MapProperty(prop.Name);
                                  var genericType = prop.PropertyType.GetGenericArguments()[0];
                                  var trackingListType = typeof(TrackingList<>).MakeGenericType(genericType);
                                  cm.MapProperty(prop.Name).SetDefaultValue(() => this.ServiceProvider.CreateInstance(trackingListType));

                                  if (prop.PropertyType.Name == typeof(ICollection<>).Name)
                                  {
                                      var collectionSerializer =
                                          typeof(TrackingICollectionSerializer<>).MakeGenericType(genericType);
                                      collectionMemberMap.SetSerializer((IBsonSerializer)this.ServiceProvider.CreateInstance(collectionSerializer));
                                  }
                                  else
                                  {
                                      var listSerializer =
                                          typeof(TrackingIListSerializer<>).MakeGenericType(genericType);
                                      collectionMemberMap.SetSerializer((IBsonSerializer)this.ServiceProvider.CreateInstance(listSerializer));
                                  }
                              }
                          }
                      });

                BsonSerializer.RegisterSerializer<T>(this.ServiceProvider.CreateInstance<ModelsSerializer<T>>());
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
