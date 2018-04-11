using System.Reflection;

namespace DbdocFramework.MongoDbProvider.Helpers
{
    internal static class  NamingConventionHelper
    {
        public static string GetNavigationPropertyName(this PropertyInfo property)
        {
            if (property.PropertyType.Name == property.Name)
            {
                return $"{property.PropertyType.Name}Id";
            }

            return $"{property.Name}_{property.PropertyType.Name}Id";
        }
    }
}
