using Dapper;
using PollMe.Api.Infrastructure;
using PollMe.Api.Models;

namespace PollMe.Api.Repositories;

public class VoteRepository(IDbConnectionFactory factory) : IVoteRepository
{
    public async Task<Vote> CreateAsync(Vote vote, IEnumerable<int> selectedOptionIds)
    {
        using var conn = factory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        var sessionToken = Guid.NewGuid().ToString();
        var voteId = await conn.ExecuteScalarAsync<int>(
            "INSERT INTO votes (poll_id, session_token, created_at) VALUES (@PollId, @SessionToken, @CreatedAt); SELECT last_insert_rowid();",
            new { vote.PollId, SessionToken = sessionToken, CreatedAt = vote.CreatedAt.ToString("o") },
            tx);

        foreach (var optionId in selectedOptionIds)
        {
            await conn.ExecuteAsync(
                "INSERT INTO vote_selections (vote_id, option_id) VALUES (@VoteId, @OptionId)",
                new { VoteId = voteId, OptionId = optionId },
                tx);
        }

        tx.Commit();
        return new Vote
        {
            Id = voteId,
            PollId = vote.PollId,
            SessionToken = sessionToken,
            CreatedAt = vote.CreatedAt
        };
    }

    public async Task<IEnumerable<OptionTally>> GetTalliesAsync(int pollId)
    {
        using var conn = factory.CreateConnection();
        return await conn.QueryAsync<OptionTally>(
            """
            SELECT o.id AS OptionId, o.text AS Text, COUNT(vs.vote_id) AS Votes
            FROM options o
            LEFT JOIN vote_selections vs ON vs.option_id = o.id
            LEFT JOIN votes v ON vs.vote_id = v.id AND v.poll_id = @PollId
            WHERE o.poll_id = @PollId
            GROUP BY o.id, o.text
            ORDER BY o.position
            """,
            new { PollId = pollId });
    }
}
