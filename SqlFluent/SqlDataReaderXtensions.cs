using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SqlFluent
{
    public static class SqlDataReaderXtensions
    {
        public static T GetSafeValue<T>(this SqlDataReader instance, string fieldName) =>
            instance[fieldName] != DBNull.Value ? instance.GetFieldValue<T>(instance.GetOrdinal(fieldName))
                                                    : default(T);

        public async static Task<T> GetSafeValueAsync<T>(this SqlDataReader instance, string fieldName) =>
            instance[fieldName] != DBNull.Value ? await instance.GetFieldValueAsync<T>(instance.GetOrdinal(fieldName))
                                                    : default(T);
    }

    public static class DictionaryExtensions {
        public static IEnumerable<T> Get<T> (this Dictionary<string, List<object>> instance, string key) {
            return instance.ContainsKey(key) ? instance[key].Select(x => (T)x) : new List<T>();
        }
    }
}
