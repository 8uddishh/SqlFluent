using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SqlFluent
{
    public interface IExecuteMulti
    {
        Dictionary<string, List<object>> ExecuteReader(Action<SqlCommand> postReadAction = null);
    }

    public interface IMultiReader
    {
        IMultiReader Reader(string key, Func<SqlDataReader, object> readerAction);
        IExecuteMulti ReadersEnd();
    }

    public interface IMulti
    {
        IMultiReader ReadersStart();
    }

    public partial interface ICommand  {
        IMulti Multi();
    }

    public partial class SqlFluent : IMulti, IMultiReader, IExecuteMulti {
        public IMulti Multi() => this;
        IMultiReader IMulti.ReadersStart() => this;
        public IMultiReader Reader(string key, Func<SqlDataReader, object> readerAction)
        {
            _multiReaders.Add(key, readerAction);
            return this;
        }
        IExecuteMulti IMultiReader.ReadersEnd() => this;

        public Dictionary<string, List<object>> ExecuteReader(Action<SqlCommand> postReadAction = null)
        {
            var result = new Dictionary<string, List<object>>();
            Execute(cmd => {
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    _multiReaders.ForEach(x => {
                        var objects = new List<object>();
                        while (reader.Read())
                        {
                            objects.Add(x.Value(reader));
                        }
                        reader.NextResult();
                        result.Add(x.Key, objects);
                    });
                }
            }, postReadAction);
            return result;
        }
    }
}
