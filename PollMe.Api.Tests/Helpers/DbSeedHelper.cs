using System.Data;
using Dapper;

namespace PollMe.Api.Tests.Helpers;

public static class DbSeedHelper
{
    public static async Task<int> InsertCreatorAsync(IDbConnection conn, string username, string passwordHash)
    {
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO creators (username, password_hash) VALUES (@Username, @PasswordHash); SELECT last_insert_rowid();",
            new { Username = username, PasswordHash = passwordHash });
    }

    public static async Task<int> InsertPollAsync(IDbConnection conn, int creatorId, string slug, string question, string mode, string visibility)
    {
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO polls (creator_id, slug, question, mode, visibility) VALUES (@CreatorId, @Slug, @Question, @Mode, @Visibility); SELECT last_insert_rowid();",
            new { CreatorId = creatorId, Slug = slug, Question = question, Mode = mode, Visibility = visibility });
    }

    public static async Task<int> InsertOptionAsync(IDbConnection conn, int pollId, string text, int position)
    {
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO options (poll_id, text, position) VALUES (@PollId, @Text, @Position); SELECT last_insert_rowid();",
            new { PollId = pollId, Text = text, Position = position });
    }

    public static async Task<int> InsertVoteAsync(IDbConnection conn, int pollId, params int[] selectedOptionIds)
    {
        var sessionToken = Guid.NewGuid().ToString();
        var voteId = await conn.ExecuteScalarAsync<int>(
            "INSERT INTO votes (poll_id, session_token, created_at) VALUES (@PollId, @SessionToken, @CreatedAt); SELECT last_insert_rowid();",
            new { PollId = pollId, SessionToken = sessionToken, CreatedAt = DateTime.UtcNow.ToString("o") });
        foreach (var optionId in selectedOptionIds)
        {
            await conn.ExecuteAsync(
                "INSERT INTO vote_selections (vote_id, option_id) VALUES (@VoteId, @OptionId)",
                new { VoteId = voteId, OptionId = optionId });
        }
        return voteId;
    }
}
