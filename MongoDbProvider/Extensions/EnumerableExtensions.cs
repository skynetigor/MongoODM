using System;
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

        public static bool Contains<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
        {
            return sequence.FirstOrDefault(predicate) != null;
        }

        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            ForEach(sequence, elem =>
            {
                action(elem);
                return true;
            });
        }

        public static void ForEach<T>(this IEnumerable<T> sequence, Func<T, bool> action)
        {
            if (sequence.GetType() == typeof(T[]))
            {
                var array = (T[])sequence;

                for (int i = 0; i < array.Length; i++)
                {
                    if (!action(array[i]))
                    {
                        return;
                    }
                }
            }
            else
            {
                foreach (var element in sequence)
                {
                    if (!action(element))
                    {
                        return;
                    }
                }
            }

        }
    }
}
