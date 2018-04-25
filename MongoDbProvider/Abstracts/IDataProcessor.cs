using System;
using System.Collections.Generic;
using System.Text;

namespace DbdocFramework.MongoDbProvider.Abstracts
{
    public interface IDataProcessor
    {
        void Add(object entity);

        void AddRange(IEnumerable<object> entities);

        void Update(object entity);

        void UpdateRange(IEnumerable<object> entities);

        void Remove(object entity);

        void RemoveRange(IEnumerable<object> entities);
    }
}
