using PollMe.Api.Models;

namespace PollMe.Api.Repositories;

public interface IVoteRepository
{
    Task<Vote> CreateAsync(Vote vote, IEnumerable<int> selectedOptionIds);
    Task<IEnumerable<OptionTally>> GetTalliesAsync(int pollId);
}
