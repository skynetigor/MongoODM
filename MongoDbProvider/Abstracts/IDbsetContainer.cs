using System;
using System.Collections.Generic;
using System.Text;
using DbdocFramework.Abstracts;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    interface IDbsetContainer
    {
        IDbSet<T> GetDbSet<T>() where T: class;
    }
}
