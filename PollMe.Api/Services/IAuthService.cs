using PollMe.Api.Models;

namespace PollMe.Api.Services;

public interface IAuthService
{
    Task<Creator> RegisterAsync(string username, string password);
    Task<Creator?> LoginAsync(string username, string password);
}
