using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DbdocFramework.MongoDbProvider.Helpers
{
    public static class  NamingConventionHelper
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
