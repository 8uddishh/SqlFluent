using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlFluent
{
    public interface IExecuteMultiAsync
    {
        Task<Dictionary<string, List<object>>> ExecuteReaderAsync(Action<SqlCommand> postReadAction = null);
    }

    public interface IMultiReaderAsync
    {
        IMultiReaderAsync ReaderAsync(string key, Func<SqlDataReader, Task<object>> readerAction);
        IExecuteMultiAsync ReadersEndAsync();
    }

    public interface IMultiAsync
    {
        IMultiReaderAsync ReadersStartAsync();
    }

    public partial interface IAsync  
    {
        IMultiAsync Multi();
    }

    public partial class SqlFluent : IMultiAsync, IMultiReaderAsync, IExecuteMultiAsync {
        IMultiAsync IAsync.Multi() => this;
        IMultiReaderAsync IMultiAsync.ReadersStartAsync() => this;

        public IMultiReaderAsync ReaderAsync(string key, Func<SqlDataReader, Task<object>> readerAction)
        {
            _multiReadersAsync.Add(key, readerAction);
            return this;
        }

        IExecuteMultiAsync IMultiReaderAsync.ReadersEndAsync() => this;

        public async Task<Dictionary<string, List<object>>> ExecuteReaderAsync(Action<SqlCommand> postReadAction = null)
        {
            var result = new Dictionary<string, List<object>>();
            await ExecuteAsync(async cmd =>
            {
                using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    _multiReadersAsync.ForEach(async x => {
                        var objects = new List<object>();
                        while (await reader.ReadAsync())
                        {
                            objects.Add(await x.Value(reader));
                        }
                        await reader.NextResultAsync();
                        result.Add(x.Key, objects);
                    });
                }
            }, postReadAction);
            return result;
        }
    }
}
