using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.MongoDbProvider.Helpers
{
    class TrackingListHelper
    {
        public static object CreateNewTrackingList(Type entityType, object enumerable)
        {
            var trListType = typeof(TrackingList<>).MakeGenericType(entityType);
            return trListType.GetMethod(nameof(CreateNewTrackingList), BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { enumerable });
        }
    }
}
