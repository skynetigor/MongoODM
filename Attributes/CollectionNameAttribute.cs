using System;
using MongoODM.Models;

namespace MongoODM.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]

    public sealed class CollectionNameAttribute : AbstractORMAttribute
    {
        public string Name { get; set; }

        protected override void Map(TypeModel model, Type currentType)
        {
            model.CollectionName = Name;
        }
    }
}
