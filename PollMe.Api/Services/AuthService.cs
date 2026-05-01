using BCrypt.Net;
using PollMe.Api.Exceptions;
using PollMe.Api.Models;
using PollMe.Api.Repositories;

namespace PollMe.Api.Services;

public class AuthService(ICreatorRepository repo) : IAuthService
{
    public async Task<Creator> RegisterAsync(string username, string password)
    {
        var existing = await repo.FindByUsernameAsync(username);
        if (existing is not null)
            throw new ConflictException("Username already taken");

        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return await repo.CreateAsync(username, hash);
    }

    public async Task<Creator?> LoginAsync(string username, string password)
    {
        var creator = await repo.FindByUsernameAsync(username);
        if (creator is null)
            return null;

        return BCrypt.Net.BCrypt.Verify(password, creator.PasswordHash) ? creator : null;
    }
}
