using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlFluent
{
    public static class IEnumerableXtensions
    {
        public static void ForEach<T> (this IEnumerable<T> instance, Action<T> action) {
            foreach (var uno in instance)
                action(uno);
        }
        public static IEnumerable<T> Get<T>(this Dictionary<string, List<object>> instance, string key)
        {
            return instance.ContainsKey(key) ? instance[key].Select(x => (T)x) : new List<T>();
        }
    }
}
