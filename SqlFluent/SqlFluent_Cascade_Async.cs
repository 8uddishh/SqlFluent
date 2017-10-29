using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlFluent
{
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
        Task<T> ExecuteSingleAsync<T>(Action<SqlCommand> postReadAction = null);
        Task<IEnumerable<T>> ExecuteReaderAsync<T>(Action<SqlCommand> postReadAction = null);
    }

    public interface ICascadeAsync
    {
        IPrimaryReaderAsync ReadersStartAsync();
    }

    public partial interface IAsync
    {
        ICascadeAsync Cascade();
    }

    public partial class SqlFluent : IPrimaryReaderAsync, ILevelReaderAsync, ICascadeAsync, IExecuteCascadeAsync 
    {
        ICascadeAsync IAsync.Cascade() => this;

        public IExecuteCascadeAsync SelectorAsync<T>(Func<SqlDataReader, Predicate<T>> selector)
        {
            _selector = selector;
            return this;
        }

        IPrimaryReaderAsync ICascadeAsync.ReadersStartAsync() => this;

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

        public async Task<T> ExecuteSingleAsync<T>(Action<SqlCommand> postReadAction = null)
        {
            var result = default(T);
            await ExecuteAsync(async cmd =>
            {
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    if (await reader.ReadAsync())
                    {
                        result = await ((Func<SqlDataReader, Task<T>>)_primaryActionAsync)(reader);

                        _levelReaderActionsAsync.ForEach(async x =>
                        {
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    await ((Func<SqlDataReader, T, Task>)x)(reader, result);
                                }
                            }
                        });
                    }
                }
            }, postReadAction);
            return result;
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(Action<SqlCommand> postReadAction = null)
        {
            var result = new List<T>();
            await ExecuteAsync(async cmd =>
            {
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    while (await reader.ReadAsync())
                        result.Add(await ((Func<SqlDataReader, Task<T>>)_primaryActionAsync)(reader));

                    _levelReaderActionsAsync.ForEach(async x =>
                    {
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
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

        IExecuteCascadeAsync ILevelReaderAsync.ReadersEndAsync() => this;
    }
}
