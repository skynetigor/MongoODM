using System.Collections.Generic;
using System.Reflection;

namespace MongoODM.Extensions
{
    internal static class  ObjectExtensions
    {
        public static IEnumerable<PropertyInfo> GetProperties(this object obj)
        {
            return obj.GetType().GetProperties();
        }

        public static PropertyInfo GetProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName);
        }
    }
}             
