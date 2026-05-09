using Dapper;
using PollMe.Api.Infrastructure;
using PollMe.Api.Models;

namespace PollMe.Api.Repositories;

public class PollRepository(IDbConnectionFactory factory) : IPollRepository
{
    public async Task<Poll> CreateWithOptionsAsync(Poll poll, IEnumerable<Option> options)
    {
        using var conn = factory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        var pollId = await conn.ExecuteScalarAsync<int>(
            "INSERT INTO polls (creator_id, question, slug, mode, visibility, created_at) VALUES (@CreatorId, @Question, @Slug, @Mode, @Visibility, @CreatedAt); SELECT last_insert_rowid();",
            new { poll.CreatorId, poll.Question, poll.Slug, Mode = poll.Mode.ToString(), Visibility = poll.Visibility.ToString(), CreatedAt = poll.CreatedAt.ToString("o") },
            tx);

        foreach (var opt in options)
        {
            await conn.ExecuteAsync(
                "INSERT INTO options (poll_id, text, position) VALUES (@PollId, @Text, @Position)",
                new { PollId = pollId, opt.Text, opt.Position },
                tx);
        }

        tx.Commit();
        return new Poll
        {
            Id = pollId,
            CreatorId = poll.CreatorId,
            Question = poll.Question,
            Slug = poll.Slug,
            Mode = poll.Mode,
            Visibility = poll.Visibility,
            CreatedAt = poll.CreatedAt
        };
    }

    public async Task<Poll?> GetBySlugAsync(string slug)
    {
        using var conn = factory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Poll>(
            "SELECT id, creator_id AS CreatorId, question, slug, mode AS Mode, visibility AS Visibility, created_at AS CreatedAt FROM polls WHERE slug = @Slug",
            new { Slug = slug });
    }

    public async Task<IEnumerable<Option>> GetOptionsByPollIdAsync(int pollId)
    {
        using var conn = factory.CreateConnection();
        return await conn.QueryAsync<Option>(
            "SELECT id, poll_id AS PollId, text, position FROM options WHERE poll_id = @PollId ORDER BY position",
            new { PollId = pollId });
    }

    public async Task<IEnumerable<Poll>> GetByCreatorIdAsync(int creatorId)
    {
        using var conn = factory.CreateConnection();
        return await conn.QueryAsync<Poll>(
            "SELECT id, creator_id AS CreatorId, question, slug, mode AS Mode, visibility AS Visibility, created_at AS CreatedAt FROM polls WHERE creator_id = @CreatorId ORDER BY created_at DESC",
            new { CreatorId = creatorId });
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        using var conn = factory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM polls WHERE slug = @Slug",
            new { Slug = slug });
        return count > 0;
    }
}
