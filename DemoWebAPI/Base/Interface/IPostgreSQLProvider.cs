using System.Data;
using Dapper;

namespace DemoWebAPI.Base.Interface
{
    public interface IPostgreSQLProvider
    {
        List<T> Query<T>(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        IEnumerable<dynamic> Query(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        IEnumerable<T> Query<T>(IDbConnection cnn, CommandDefinition command);

        IDbCommand CreateCommand(IDbConnection cnn, string sql, object param, CommandType commandType, int commandTimeout, bool isProcessDBNull = false);

        int Execute(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        int Execute(IDbConnection cnn, CommandDefinition command);
    }
}
