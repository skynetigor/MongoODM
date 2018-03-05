using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace MongoODM.Extensions
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<T> Concat<T>(this IEnumerable<IEnumerable<T>> sequence)
        {
            IEnumerable<T> result = null;

            foreach (var seq in sequence)
            {
                if (result == null)
                {
                    result = seq;
                    continue;
                }

                result = result.Concat(seq);
            }

            return result;
        }
    }
}
