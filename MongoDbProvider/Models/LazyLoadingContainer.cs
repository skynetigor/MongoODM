using System;
using System.Collections.Generic;
using System.Text;

namespace DbdocFramework.MongoDbProvider.Models
{
    class LazyLoadingResult<T>
    {
        public LazyLoadingResult(IEnumerable<T> result)
        {
            this.Result = result;
        }

        public IEnumerable<T> Result { get; }
    }
}
