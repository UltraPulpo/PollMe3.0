using System.Data;
namespace PollMe.Api.Infrastructure;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
