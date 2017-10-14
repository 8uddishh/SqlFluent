using System;
using System.Collections.Generic;

namespace SqlFluent
{
    public static class IEnumerableXtensions
    {
        public static void ForEach<T> (this IEnumerable<T> instance, Action<T> action) {
            foreach (var uno in instance)
                action(uno);
        }
    }
}
