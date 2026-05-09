using PollMe.Api.Models;

namespace PollMe.Api.Repositories;

public interface IPollRepository
{
    Task<Poll> CreateWithOptionsAsync(Poll poll, IEnumerable<Option> options);
    Task<Poll?> GetBySlugAsync(string slug);
    Task<IEnumerable<Option>> GetOptionsByPollIdAsync(int pollId);
    Task<IEnumerable<Poll>> GetByCreatorIdAsync(int creatorId);
    Task<bool> SlugExistsAsync(string slug);
}
