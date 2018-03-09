using System;
using MongoODM.DI.Abstract;
using MongoODM.Models;

namespace MongoODM.Abstracts
{
    interface ITypeMetadataInitializer
    {
        void Map(TypeMetadata model, Type currentType, ICustomServiceProvider serviceProvider);
    }
}
