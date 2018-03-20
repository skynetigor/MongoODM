using System;
using System.Collections.Generic;
using System.Text;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    interface IDataLoadersProvider
    {
        IDataLoader<TResult> GetDataLoader<TResult>();
    }
}
