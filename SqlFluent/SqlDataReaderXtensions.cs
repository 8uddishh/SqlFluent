using System;
using System.Data.SqlClient;

namespace SqlFluent
{
    public static class SqlDataReaderXtensions
    {
        public static T GetSafeValue<T>(this SqlDataReader instance, string fieldName) =>
            instance[fieldName] != DBNull.Value ? instance.GetFieldValue<T>(instance.GetOrdinal(fieldName))
                                                    : default(T);
    }
}
