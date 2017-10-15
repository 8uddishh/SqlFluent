using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlFluent
{
    public partial class SqlFluent
    {
        async Task ExecuteAsync(Func<SqlCommand, Task> actionAsync, Action<SqlCommand> postAction = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(_commandText, connection) { CommandType = _commandType })
                {
                    await connection.OpenAsync();
                    if (_commandParameters.Count > 0)
                        command.Parameters.AddRange(_commandParameters.ToArray());

                    await actionAsync(command);
                    postAction?.Invoke(command);
                }
            }
        }

        public async Task ExecuteNonQueryAsync(Action<SqlCommand> postAction = null)
        {
            await ExecuteAsync(cmd => cmd.ExecuteNonQueryAsync(), postAction);
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(Func<SqlDataReader, Task<T>> readerActionAsync, 
                                                                Action<SqlCommand> postAction = null)
        {
            var result = new List<T>();
            await ExecuteAsync(async cmd => {
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    while (await reader.ReadAsync())
                        result.Add(await readerActionAsync(reader));
                }
            }, postAction);

            return result;
        }

        public async Task<T> ExecuteSingleAsync<T>(Func<SqlDataReader, Task<T>> readerActionAsync,
                                                                Action<SqlCommand> postAction = null) {
            var result = default(T);
            await ExecuteAsync(async cmd => {
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    while (await reader.ReadAsync())
                        result = await readerActionAsync(reader);
                }
            }, postAction);

            return result;
        }

        public async Task<object> ExecuteScalarAsync(Action<SqlCommand> postAction = null)
        {
            object result = null;
            await ExecuteAsync(async cmd => {
                result = await cmd.ExecuteScalarAsync();
            }, postAction);
            return result;
        }
    }
}
