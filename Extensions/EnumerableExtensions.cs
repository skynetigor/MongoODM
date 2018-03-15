using System.Collections.Generic;
using System.Linq;

namespace DbdocFramework.Extensions
{
    internal static class EnumerableExtensions
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
