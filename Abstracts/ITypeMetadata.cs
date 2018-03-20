using System;
using DbdocFramework.DI.Abstract;
using DbdocFramework.MongoDbProvider.Models;

namespace DbdocFramework.Abstracts
{
    interface ITypeMetadata
    {
        void Map(TypeMetadata model, Type currentType, ICustomServiceProvider serviceProvider);
    }
}
