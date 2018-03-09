using System;
using MongoODM.Abstracts;
using MongoODM.DI.Abstract;
using MongoODM.Models;

namespace MongoODM.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]

    public sealed class CollectionNameAttribute : AbstractORMAttribute
    {
        public CollectionNameAttribute()
        {

        }

        public string Name { get; set; }

        public override void Map(TypeMetadata model, Type currentType, ICustomServiceProvider serviceProvider)
        {
            model.CollectionName = Name;
        }
    }
}
