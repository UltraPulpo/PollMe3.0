using PollMe.Api.Models;

namespace PollMe.Api.Repositories;

public interface ICreatorRepository
{
    Task<Creator?> FindByUsernameAsync(string username);
    Task<Creator> CreateAsync(string username, string passwordHash);
}
