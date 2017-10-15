using System;
using System.Data.SqlClient;
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
}
