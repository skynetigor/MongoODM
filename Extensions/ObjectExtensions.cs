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

        public static IEnumerable<MethodInfo> GetMethods(this object obj)
        {
            return obj.GetType().GetMethods();
        }

        public static MethodInfo GetMethod(this object obj, string methodName)
        {
            return obj.GetType().GetMethod(methodName);
        }
    }
}             
