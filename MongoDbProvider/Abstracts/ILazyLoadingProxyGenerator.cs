using System;
using System.Collections.Generic;
using System.Text;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    interface ILazyLoadingProxyGenerator
    {
        T CreateProxy<T>(T target);

        IEnumerable<T> CreateProxies<T>(IEnumerable<T> targets);
    }
}
