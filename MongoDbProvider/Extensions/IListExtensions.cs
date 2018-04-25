using System;
using System.Collections.Generic;
using System.Text;
using DbdocFramework.Extensions;

namespace DbdocFramework.MongoDbProvider.Extensions
{
    internal static class ICollectionExtensions
    {
        public static int RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> range)
        {
            int count = 0;

            range.ForEach(e =>
            {
                if (collection.Remove(e))
                {
                    count++;
                }
            });

            return count;
        }
    }
}
