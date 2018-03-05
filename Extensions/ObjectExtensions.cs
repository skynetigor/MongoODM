using System.Collections.Generic;
using System.Reflection;

namespace MongoORM.Extensions
{
    internal static class  ObjectExtensions
    {
        public static IEnumerable<PropertyInfo> GetProperties(this object obj)
        {
            return obj.GetType().GetProperties(BindingFlags.Instance);
        }

        public static PropertyInfo GetProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName);
        }
    }
}             
