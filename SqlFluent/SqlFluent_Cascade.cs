using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SqlFluent
{
    public interface ICascade
    {
        IPrimaryReader ReadersStart();
    }

    public interface ILevelReader
    {
        ILevelReader Reader<T>(Action<SqlDataReader, T> readerAction);
        IExecuteCascade ReadersEnd();
    }
    public interface IPrimaryReader
    {

        ILevelReader Reader<T>(Func<SqlDataReader, T> readerAction);
    }


    public interface IExecuteCascade
    {
        IExecuteCascade Selector<T>(Func<SqlDataReader, Predicate<T>> selector);
        T ExecuteSingle<T>(Action<SqlCommand> postReadAction = null);
        IEnumerable<T> ExecuteReader<T>(Action<SqlCommand> postReadAction = null);
    }

    public partial interface ICommand {
        ICascade Cascade();
    }

    public partial class SqlFluent : IPrimaryReader, ILevelReader, ICascade, IExecuteCascade {
        object _primaryAction;
        List<object> _levelReaderActions = new List<object>();
        object _selector;

        IPrimaryReader ICascade.ReadersStart() => this;
        ICascade ICommand.Cascade() => this;

        public ILevelReader Reader<T>(Func<SqlDataReader, T> readerAction)
        {
            _primaryAction = readerAction;
            return this;
        }

        public ILevelReader Reader<T>(Action<SqlDataReader, T> readerAction)
        {
            _levelReaderActions.Add(readerAction);
            return this;
        }

        public T ExecuteSingle<T>(Action<SqlCommand> postReadAction = null)
        {
            var result = default(T);
            Execute(cmd =>
            {
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (reader.Read())
                    {
                        result = ((Func<SqlDataReader, T>)_primaryAction)(reader);

                        _levelReaderActions.ForEach(x =>
                        {
                            if (reader.NextResult())
                                while (reader.Read())
                                    ((Action<SqlDataReader, T>)x)(reader, result);
                        });
                    }
                }
                postReadAction?.Invoke(cmd);
            });
            return result;
        }

        public IExecuteCascade Selector<T>(Func<SqlDataReader, Predicate<T>> selector)
        {
            _selector = selector;
            return this;
        }

        public IEnumerable<T> ExecuteReader<T>(Action<SqlCommand> postReadAction = null)
        {
            var result = new List<T>();
            Execute(cmd =>
            {
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                        result.Add(((Func<SqlDataReader, T>)_primaryAction)(reader));

                    _levelReaderActions.ForEach(x =>
                    {
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                var selector = ((Func<SqlDataReader, Predicate<T>>)_selector);
                                var selected = result.Find(selector(reader));
                                if (selected != null)
                                    ((Action<SqlDataReader, T>)x)(reader, selected);
                            }
                        }
                    });
                }
                postReadAction?.Invoke(cmd);
            });

            return result;
        }

        public IExecuteCascade ReadersEnd() => this;
    }
}
