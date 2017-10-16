using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlFluent
{
    public interface ICommand
    {
        ICommand ParameterIf(Func<bool> condition, string name, SqlDbType type, object value = null, int? size = default(int?),
                              ParameterDirection direction = ParameterDirection.Input);
        ICommand Parameter(string name, SqlDbType type, object value = null, int? size = default(int?),
                              ParameterDirection direction = ParameterDirection.Input);
        ICommand Query(string query);
        ICommand StoredProcedure(string query);

        void ExecuteNonQuery(Action<SqlCommand> action = null);
        IEnumerable<T> ExecuteReader<T>(Func<SqlDataReader, T> readerAction, Action<SqlCommand> postReadAction = null);
        IEnumerable<T> ExecuteReaderWithYield<T>(Func<SqlDataReader, T> readerAction);
        T ExecuteSingle<T>(Func<SqlDataReader, T> readerAction, Action<SqlCommand> postReadAction = null);
        object ExecuteScalar(Action<SqlCommand> postReadAction = null);


        Task ExecuteNonQueryAsync(Action<SqlCommand> postAction = null);
        Task<IEnumerable<T>> ExecuteReaderAsync<T>(Func<SqlDataReader, Task<T>> readerActionAsync,
                                                   Action<SqlCommand> postAction = null);
        Task<T> ExecuteSingleAsync<T>(Func<SqlDataReader, Task<T>> readerActionAsync,
                                            Action<SqlCommand> postAction = null);
        Task<object> ExecuteScalarAsync(Action<SqlCommand> postAction = null);

    }

    public interface IConnectionString
    {
        ICommand ConnectionString(string connectionString);
    }

    public partial class SqlFluent : IConnectionString, ICommand
    {
        string _connectionString;
        List<SqlParameter> _commandParameters;
        string _commandText;
        CommandType _commandType;

        public SqlFluent() {
            _commandParameters = new List<SqlParameter>();
        }

        public SqlFluent(string connectionString) : this()
        {
            _connectionString = connectionString;
        }

        public ICommand ConnectionString (string connectionString) {
            _connectionString = connectionString;
            return this;
        }

        public ICommand Query(string query) {
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

        public ICommand ParameterIf(Func<bool> condition, string name, SqlDbType type, object value = null, int? size = default(int?),
                                    ParameterDirection direction = ParameterDirection.Input) {
            if (condition())
                return Parameter(name, type, value, size, direction);

            return this;
        }

        public ICommand Parameter(string name, SqlDbType type, object value = null, int? size = default(int?),
                           ParameterDirection direction = ParameterDirection.Input) {
            var parameter = new SqlParameter { ParameterName = name, SqlDbType = type, Direction = direction };
            if (size.HasValue)
                parameter.Size = size.Value;
            if (value != null)
                parameter.Value = value;
            _commandParameters.Add(parameter);
            return this;
        }

        void Execute(Action<SqlCommand> action) {
            using(var connection = new SqlConnection(_connectionString)) {
                using(var command = new SqlCommand(_commandText, connection) { CommandType = _commandType } ) {
                    connection.Open();
                    if (_commandParameters.Count > 0)
                        command.Parameters.AddRange(_commandParameters.ToArray());

                    action(command);
                    connection.Close(); 
                }
            }
        }

        public void ExecuteNonQuery(Action<SqlCommand> action = null) {
            Execute(cmd => {
                cmd.ExecuteNonQuery();
                action?.Invoke(cmd);
            });
        }

        public IEnumerable<T> ExecuteReader<T> (Func<SqlDataReader, T> readerAction, Action<SqlCommand> postReadAction = null) {
            var result = new List<T>();
            Execute(cmd => {
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection)){
                    while (reader.Read())
                        result.Add(readerAction(reader));
                }
                postReadAction?.Invoke(cmd); 
            });

            return result;
        }

        public IEnumerable<T> ExecuteReaderWithYield<T> (Func<SqlDataReader, T> readerAction) {
            using (var connection = new SqlConnection(_connectionString)) {
                using (var command = new SqlCommand(_commandText, connection) { CommandType = _commandType }) {
                    connection.Open();
                    if (_commandParameters.Count > 0)
                        command.Parameters.AddRange(_commandParameters.ToArray());

                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection)) {
                        while (reader.Read())
                            yield return readerAction(reader);
                    }
                    connection.Close(); 
                }
            }
        }

        public T ExecuteSingle<T> (Func<SqlDataReader, T> readerAction, Action<SqlCommand> postReadAction = null) {
            var result = default(T);
            Execute(cmd => {
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection)) {
                    while (reader.Read()) {
                        result = readerAction(reader);
                        break; 
                    }   
                }
                postReadAction?.Invoke(cmd);
            });

            return result;
        }

        public object ExecuteScalar(Action<SqlCommand> postReadAction = null) {
            object result = null;
            Execute(cmd => {
                result = cmd.ExecuteScalar();
                postReadAction?.Invoke(cmd);
            });
            return result;
        }
    }
}
