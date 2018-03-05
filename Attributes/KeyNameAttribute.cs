using System;
using MongoORM.Models;

namespace MongoORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]

    public class KeyNameAttribute : AbstractORMAttribute
    {
        public string Name { get; set; }

        protected override void Map(TypeModel model)
        {
            model.IdName = Name;
        }
    }
}
