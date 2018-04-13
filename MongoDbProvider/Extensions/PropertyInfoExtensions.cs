using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbdocFramework.Extensions;

namespace DbdocFramework.MongoDbProvider.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static bool IsIEnumerableType(this Type type)
        {
            return type.GetInterfaces().Contains(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }
    }
}
