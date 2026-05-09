using System.Data;
using Microsoft.Data.Sqlite;
namespace PollMe.Api.Infrastructure;

public class DbConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new SqliteConnection(connectionString);
}
