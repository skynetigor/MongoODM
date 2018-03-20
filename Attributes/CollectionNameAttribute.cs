using System;
using DbdocFramework.Abstracts;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.Attributes
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
