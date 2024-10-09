using Dapper;
using Microsoft.Data.SqlClient;
using TyreCompare.DAL.Interfaces;
using TyreCompare.Models;

namespace TyreCompare.DAL.Dapper;

public class LoggingRepository_Dapper : ILoggingRepository
{
    private string ConnectionString;

    public LoggingRepository_Dapper(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public async Task<bool> AddLog(Models.Log logObject)
    {
        var query = $@"  INSERT INTO [App].[Logs]
                        (LogLevel, Message, Trace)
                        VALUES
                        (@logLevel, @message, @trace)";

        var logData = new
        {
            logLevel = logObject.LogLevel,
            message = logObject.Message,
            trace = logObject.Trace
        };

        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        var result = await connection.ExecuteAsync(query, logData);

        return (result > 0);
    }
}
