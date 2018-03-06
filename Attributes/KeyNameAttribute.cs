using System;
using System.Linq;
using MongoODM.Models;

namespace MongoODM.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]

    public class KeyNameAttribute : AbstractORMAttribute
    {
        protected override void Map(TypeModel model, Type currentType)
        {
            model.IdProperty = currentType.GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyNameAttribute), false) != null);
        }
    }
}
