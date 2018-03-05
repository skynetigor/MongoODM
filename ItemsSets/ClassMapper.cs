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
        private readonly MethodInfo _mapClassGenericMethod;

        public ClassMapper(ITypeInitializer typeInitializer)
        {
            this._typeInitializer = typeInitializer;
            this._mapClassGenericMethod = this.GetMethods()
                .FirstOrDefault(m => m.Name == nameof(this.MapClass));
        }

        public void MapClass<T>()
        {
            var type = typeof(T);
            BsonSerializer.RegisterSerializer<ICollection<T>>(new TrackingICollectionSerializer<T>());
            BsonSerializer.RegisterSerializer<IList<T>>(new TrackingIListSerializer<T>());

            if (!BsonClassMap.IsClassMapRegistered(type))
            {
                BsonClassMap.RegisterClassMap<T>(
                      cm =>
                      {
                          cm.AutoMap();
                          cm.SetIgnoreExtraElements(true);
                          foreach (var prop in type.GetProperties())
                          {
                              if (prop.PropertyType.Name == typeof(ICollection<>).Name || prop.PropertyType.Name == typeof(IList<>).Name)
                              {
                                  var genericType = prop.PropertyType.GetGenericArguments()[0];
                                  var trackingListType = typeof(TrackingList<>).MakeGenericType(genericType);
                                  cm.MapProperty(prop.Name).SetDefaultValue(() => Activator.CreateInstance(trackingListType));
                              }
                          }
                      });
            }
        }

        public void MapClass(Type type)
        {
            this._mapClassGenericMethod.MakeGenericMethod(type).Invoke(this, null);
        }
    }
}
