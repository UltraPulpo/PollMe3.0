using Dapper;
using PollMe.Api.Infrastructure;
using PollMe.Api.Models;

namespace PollMe.Api.Repositories;

public class CreatorRepository(IDbConnectionFactory factory) : ICreatorRepository
{
    public async Task<Creator?> FindByUsernameAsync(string username)
    {
        using var conn = factory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Creator>(
            "SELECT id, username, password_hash AS PasswordHash, created_at AS CreatedAt FROM creators WHERE username = @Username",
            new { Username = username });
    }

    public async Task<Creator> CreateAsync(string username, string passwordHash)
    {
        using var conn = factory.CreateConnection();
        var id = await conn.ExecuteScalarAsync<int>(
            "INSERT INTO creators (username, password_hash, created_at) VALUES (@Username, @PasswordHash, @CreatedAt); SELECT last_insert_rowid();",
            new { Username = username, PasswordHash = passwordHash, CreatedAt = DateTime.UtcNow.ToString("o") });
        return new Creator { Id = id, Username = username, PasswordHash = passwordHash, CreatedAt = DateTime.UtcNow };
    }
}
