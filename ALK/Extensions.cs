using System;
using System.Linq;
using System.Collections.Generic;

namespace ALK
{
    public static class Extensions
    {
        private static readonly Random _random;

        static Extensions()
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
        }

        public static T RandomItem<T>(this IList<T> items)
            => items[_random.Next(items.Count)];
    }
}