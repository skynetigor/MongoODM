using System;
using System.Linq;
using MongoODM.DI.Abstract;
using MongoODM.Models;

namespace MongoODM.Attributes
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
