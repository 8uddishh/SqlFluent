using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlFluent
{
    public interface ILevelReader {
        ILevelReader Reader<T>(Action<SqlDataReader, T> readerAction);
        IExecuteCascade ReadersEnd();
    }
    public interface IPrimaryReader {
        ILevelReader Reader<T>(Func<SqlDataReader, T> readerAction);
    }

    public interface IParameter {
        IParameter ParameterIf(Func<bool> condition, string name, SqlDbType type, object value = null, int? size = default(int?),
                              ParameterDirection direction = ParameterDirection.Input);
        IParameter Parameter(string name, SqlDbType type, object value = null, int? size = default(int?),
                              ParameterDirection direction = ParameterDirection.Input);
        ICommand ParametersEnd();
    }

    public interface ICommand
    {
        IParameter ParametersStart();
        IPrimaryReader ReadersStart();
        ICommand Query(string query);
        ICommand StoredProcedure(string query);

        void ExecuteNonQuery(Action<SqlCommand> action = null);
        IEnumerable<T> ExecuteReader<T>(Func<SqlDataReader, T> readerAction, Action<SqlCommand> postReadAction = null);
        IEnumerable<T> ExecuteReaderWithYield<T>(Func<SqlDataReader, T> readerAction);
        T ExecuteSingle<T>(Func<SqlDataReader, T> readerAction, Action<SqlCommand> postReadAction = null);
        object ExecuteScalar(Action<SqlCommand> postReadAction = null);


        IAsync Async();

    }

    public interface IConnectionString
    {
        ICommand ConnectionString(string connectionString);
    }

    public interface IExecuteCascade
    {
        IExecuteCascade Selector<T>(Func<SqlDataReader, Predicate<T>> selector);
        T ExecuteSingleWithCascade<T>(Action<SqlCommand> postReadAction = null);
        IEnumerable<T> ExecuteReaderWithCascade<T>( Action<SqlCommand> postReadAction = null);
    }

    public partial class SqlFluent : IConnectionString, ICommand, IParameter,
        IPrimaryReader, ILevelReader, IExecuteCascade
    {
        string _connectionString;
        List<SqlParameter> _commandParameters;
        string _commandText;
        CommandType _commandType;
        object _primaryAction;
        List<object> _levelReaderActions = new List<object>();
        object _selector;

        public SqlFluent()
        {
            _commandParameters = new List<SqlParameter>();
        }

        public SqlFluent(string connectionString) : this()
        {
            _connectionString = connectionString;
        }

        public ICommand ConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            return this;
        }

        public ICommand Query(string query)
        {
            _commandText = query;
            _commandType = CommandType.Text;
            return this;
        }

        public ICommand StoredProcedure(string query)
        {
            _commandText = query;
            _commandType = CommandType.StoredProcedure;
            return this;
        }

        public IParameter ParametersStart() => this;

        public IParameter ParameterIf(Func<bool> condition, string name, SqlDbType type, object value = null, int? size = default(int?),
                                    ParameterDirection direction = ParameterDirection.Input)
        {
            if (condition())
                return Parameter(name, type, value, size, direction);

            return this;
        }

        public IParameter Parameter(string name, SqlDbType type, object value = null, int? size = default(int?),
                           ParameterDirection direction = ParameterDirection.Input)
        {
            var parameter = new SqlParameter { ParameterName = name, SqlDbType = type, Direction = direction };
            if (size.HasValue)
                parameter.Size = size.Value;
            if (value != null)
                parameter.Value = value;
            _commandParameters.Add(parameter);
            return this;
        }

        public ICommand ParametersEnd() => this;

        void Execute(Action<SqlCommand> action)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(_commandText, connection) { CommandType = _commandType })
                {
                    connection.Open();
                    if (_commandParameters.Count > 0)
                        command.Parameters.AddRange(_commandParameters.ToArray());

                    action(command);
                    connection.Close();
                }
            }
        }

        public void ExecuteNonQuery(Action<SqlCommand> action = null)
        {
            Execute(cmd =>
            {
                cmd.ExecuteNonQuery();
                action?.Invoke(cmd);
            });
        }

        public IEnumerable<T> ExecuteReader<T>(Func<SqlDataReader, T> readerAction, Action<SqlCommand> postReadAction = null)
        {
            var result = new List<T>();
            Execute(cmd =>
            {
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                        result.Add(readerAction(reader));
                }
                postReadAction?.Invoke(cmd);
            });

            return result;
        }

        public IEnumerable<T> ExecuteReaderWithYield<T>(Func<SqlDataReader, T> readerAction)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(_commandText, connection) { CommandType = _commandType })
                {
                    connection.Open();
                    if (_commandParameters.Count > 0)
                        command.Parameters.AddRange(_commandParameters.ToArray());

                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                            yield return readerAction(reader);
                    }
                    connection.Close();
                }
            }
        }

        public T ExecuteSingle<T>(Func<SqlDataReader, T> readerAction, Action<SqlCommand> postReadAction = null)
        {
            var result = default(T);
            Execute(cmd =>
            {
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        result = readerAction(reader);
                        break;
                    }
                }
                postReadAction?.Invoke(cmd);
            });

            return result;
        }

        public object ExecuteScalar(Action<SqlCommand> postReadAction = null)
        {
            object result = null;
            Execute(cmd =>
            {
                result = cmd.ExecuteScalar();
                postReadAction?.Invoke(cmd);
            });
            return result;
        }

        #region Cascade
        public IPrimaryReader ReadersStart() => this;

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

        public T ExecuteSingleWithCascade<T>(Action<SqlCommand> postReadAction = null)
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

        public IEnumerable<T> ExecuteReaderWithCascade<T>(Action<SqlCommand> postReadAction = null)
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
        #endregion
    }
}
