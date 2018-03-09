using MongoODM.Models;
using System;
using System.Collections.Generic;
using System.Text;
using MongoODM.Abstracts;
using MongoODM.DI.Abstract;

namespace MongoODM.Attributes
{
    public abstract class AbstractORMAttribute : Attribute, ITypeMetadataInitializer
    {
        public abstract void Map(TypeMetadata model, Type currentType, ICustomServiceProvider serviceProvider);
    }
}
