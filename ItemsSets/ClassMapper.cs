using MongoDB.Bson.Serialization;
using MongoODM.Abstracts;
using MongoODM.Models;
using MongoODM.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoODM.ItemsSets
{
    internal class ClassMapper : IClassMapper
    {
        private ITypeInitializer typeInitializer;

        public ClassMapper(ITypeInitializer typeInitializer)
        {
            this.typeInitializer = typeInitializer;
        }

        public void MapClass<T>()
        {
            var type = typeof(T);
            var tmodel = typeInitializer.GetTypeModel(type);
            BsonSerializer.RegisterSerializer<ICollection<T>>(new TrackingICollectionSerializer<T>());
            BsonSerializer.RegisterSerializer<IList<T>>(new TrackingIListSerializer<T>());

            if (!BsonClassMap.IsClassMapRegistered(type))
            {
                BsonClassMap.RegisterClassMap<T>(
                      cm =>
                      {
                          cm.AutoMap();
                          cm.SetIgnoreExtraElements(true);
                          foreach(var prop in type.GetProperties())
                          {
                              if(prop.PropertyType.Name == typeof(ICollection<>).Name || prop.PropertyType.Name == typeof(IList<>).Name)
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
            var initTypeMethod = this.GetType()
                .GetMethods()
                .FirstOrDefault(m => m.Name == nameof(this.MapClass) && m.GetGenericArguments()
                .Count() == 1)
                .MakeGenericMethod(type);
            initTypeMethod.Invoke(this, null);
        }
    }
}
