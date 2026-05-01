using PollMe.Api.Dtos;
using PollMe.Api.Models;

namespace PollMe.Api.Services;

public interface IVoteService
{
    Task<TallyDto> SubmitVoteAsync(Poll poll, IEnumerable<int> selectedOptionIds);
    Task<TallyDto> GetTalliesAsync(int pollId);
}
