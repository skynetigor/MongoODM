using System;
using System.Collections.Generic;
using System.Text;
using DbdocFramework.Abstracts;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.Attributes
{
    public abstract class AbstractORMAttribute : Attribute, ITypeMetadata
    {
        public abstract void Map(TypeMetadata model, Type currentType, ICustomServiceProvider serviceProvider);
    }
}
