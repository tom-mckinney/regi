using System.Collections.Generic;
using System.Linq;

namespace Regi.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
        {
            return self.Select((item, index) => (item, index));
        }

        public static bool ContainsAny<T>(this IEnumerable<T> self, IEnumerable<T> items)
        {
            foreach (var i in items)
            {
                if (self.Contains(i))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsAll<T>(this IEnumerable<T> self, IEnumerable<T> items)
        {
            foreach (var i in items)
            {
                if (!self.Contains(i))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TryAdd<T>(this ICollection<T> self, T item)
        {
            if (self.Contains(item))
            {
                return false;
            }

            self.Add(item);

            return true;
        }
    }
}
