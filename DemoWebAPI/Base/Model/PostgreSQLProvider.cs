using Dapper;
using DemoWebAPI.Base.Interface;
using Npgsql;
using System.Data;

namespace DemoWebAPI.Base.Model
{
    public class PostgreSQLProvider : IPostgreSQLProvider
    {
        public IDbCommand CreateCommand(IDbConnection cnn, string sql, object param, CommandType commandType, int commandTimeout, bool isProcessDBNull = false)
        {
            IDbCommand cmd = cnn.CreateCommand();
            ValidateScript(sql);
            cmd.CommandType = commandType;
            cmd.CommandText = sql;
            return cmd;
        }

        private void ValidateScript(string sql)
        {
            if (!string.IsNullOrEmpty(sql) && sql.Contains("'"))
            {
                throw new Exception("Script not allow containt charater <'>");
            }
        }

        public int Execute(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            ValidateScript(sql);
            return cnn.Execute(sql, param, transaction, commandTimeout, commandType);
        }

        public int Execute(IDbConnection cnn, CommandDefinition command)
        {
            return cnn.Execute(command);
        }

        public List<T> Query<T>(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            ValidateScript(sql);
            return cnn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType).ToList();
        }

        public IEnumerable<dynamic> Query(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            ValidateScript(sql);
            return cnn.Query(sql, param, transaction, buffered, commandTimeout, commandType);
        }

        public IEnumerable<T> Query<T>(IDbConnection cnn, CommandDefinition command)
        {
            return cnn.Query<T>(command);
        }

        public IDbConnection GetConnection(string cnnString)
        {
            return new NpgsqlConnection(cnnString);
        }
    }
}
