using System;
using System.Collections.Generic;
using System.Text;
using DbdocFramework.Abstracts;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    interface ILazyLoadingIncludableQueryable<T>: IIncludableQueryable<T>
    {
    }
}
