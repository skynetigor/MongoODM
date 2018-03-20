using System;
using System.Linq;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]

    public class KeyNameAttribute : AbstractORMAttribute
    {
        public override void Map(TypeMetadata model, Type currentType, ICustomServiceProvider serviceProvider)
        {
            model.IdProperty = currentType.GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyNameAttribute), false) != null);
        }
    }
}
