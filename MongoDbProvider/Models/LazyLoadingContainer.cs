using System;
using System.Collections.Generic;
using System.Text;

namespace DbdocFramework.MongoDbProvider.Models
{
    class LazyLoadingResult<T>
    {
        public LazyLoadingResult(T result)
        {
            this.Result = result;
        }

        public T Result { get; }
    }
}
