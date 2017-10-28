using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlFluent
{
    public interface IAsync {
        IPrimaryReaderAsync ReadersStartAsync();
        Task ExecuteNonQueryAsync(Action<SqlCommand> postAction = null);
        Task<IEnumerable<T>> ExecuteReaderAsync<T>(Func<SqlDataReader, Task<T>> readerActionAsync,
                                                   Action<SqlCommand> postAction = null);
        Task<T> ExecuteSingleAsync<T>(Func<SqlDataReader, Task<T>> readerActionAsync,
                                            Action<SqlCommand> postAction = null);
        Task<object> ExecuteScalarAsync(Action<SqlCommand> postAction = null);
    }

    public interface ILevelReaderAsync
    {
        ILevelReaderAsync ReaderAsync<T>(Func<SqlDataReader, T, Task> readerActionAsync);
        IExecuteCascadeAsync ReadersEndAsync();
    }
    public interface IPrimaryReaderAsync
    {
        ILevelReaderAsync ReaderAsync<T>(Func<SqlDataReader, Task<T>> readerActionAsync);
    }

    public interface IExecuteCascadeAsync
    {
        IExecuteCascadeAsync SelectorAsync<T>(Func<SqlDataReader, Predicate<T>> selector);
        Task<T> ExecuteSingleWithCascadeAsync<T>(Action<SqlCommand> postReadAction = null);
        Task<IEnumerable<T>> ExecuteReaderWithCascadeAsync<T>(Action<SqlCommand> postReadAction = null);
    }

    public partial class SqlFluent : IAsync, IPrimaryReaderAsync, ILevelReaderAsync, IExecuteCascadeAsync
    {
        object _primaryActionAsync;
        List<object> _levelReaderActionsAsync = new List<object>();

        public IAsync Async() => this;

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
                    connection.Close();
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
            await ExecuteAsync(async cmd =>
            {
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    while (await reader.ReadAsync())
                        result.Add(await readerActionAsync(reader));
                }
            }, postAction);

            return result;
        }

        public async Task<T> ExecuteSingleAsync<T>(Func<SqlDataReader, Task<T>> readerActionAsync,
                                                                Action<SqlCommand> postAction = null)
        {
            var result = default(T);
            await ExecuteAsync(async cmd =>
            {
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
            await ExecuteAsync(async cmd =>
            {
                result = await cmd.ExecuteScalarAsync();
            }, postAction);
            return result;
        }

        #region Async Cascade 
        public IExecuteCascadeAsync SelectorAsync<T>(Func<SqlDataReader, Predicate<T>> selector) {
            _selector = selector;
            return this;
        }
        public IPrimaryReaderAsync ReadersStartAsync() => this;

        public ILevelReaderAsync ReaderAsync<T>(Func<SqlDataReader, Task<T>> readerActionAsync)
        {
            _primaryActionAsync = readerActionAsync;
            return this;
        }

        public ILevelReaderAsync ReaderAsync<T>(Func<SqlDataReader, T, Task> readerActionAsync)
        {
            _levelReaderActionsAsync.Add(readerActionAsync);
            return this;
        }

        public async Task<T> ExecuteSingleWithCascadeAsync<T>(Action<SqlCommand> postReadAction = null)
        {
            var result = default(T);
            await ExecuteAsync(async cmd => {
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    if (await reader.ReadAsync())
                    {
                        result = await ((Func<SqlDataReader, Task<T>>)_primaryActionAsync)(reader);

                        _levelReaderActionsAsync.ForEach(async x => {
                            if (await reader.NextResultAsync()) {
                                while (await reader.ReadAsync()) {
                                    await ((Func<SqlDataReader, T, Task>)x)(reader, result);
                                }
                            }
                        });
                    }
                }
            }, postReadAction);
            return result;
        }

        public async Task<IEnumerable<T>> ExecuteReaderWithCascadeAsync<T>(Action<SqlCommand> postReadAction = null)
        {
            var result = new List<T>();
            await ExecuteAsync(async cmd => {
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    while (await reader.ReadAsync())
                        result.Add(await ((Func<SqlDataReader, Task<T>>)_primaryActionAsync)(reader));

                    _levelReaderActionsAsync.ForEach(async x => {
                        if (await reader.NextResultAsync()) {
                            while(await reader.ReadAsync())
                            {
                                var selector = ((Func<SqlDataReader, Predicate<T>>)_selector);
                                var selected = result.Find(selector(reader));
                                if (selected != null)
                                    await ((Func<SqlDataReader, T, Task>)x)(reader, selected);
                            }
                        }
                    });
                }
                postReadAction?.Invoke(cmd);
            });

            return result;
        }

        public IExecuteCascadeAsync ReadersEndAsync() => this;

        #endregion
    }
}
