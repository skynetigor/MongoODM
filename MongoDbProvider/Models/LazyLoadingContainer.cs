using System;
using System.Collections.Generic;
using System.Text;

namespace DbdocFramework.MongoDbProvider.Models
{
    class LazyLoadingContainer<T>
    {
        public LazyLoadingContainer(T proxy, T target)
        {
            this.Target = target;
            this.Proxy = proxy;
        }

        public T Proxy { get; }

        public T Target { get; }
    }
}
