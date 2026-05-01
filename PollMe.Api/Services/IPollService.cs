using PollMe.Api.Dtos;
using PollMe.Api.Models;

namespace PollMe.Api.Services;

public interface IPollService
{
    Task<Poll> CreatePollAsync(int creatorId, CreatePollRequest request);
    Task<Poll> GetBySlugAsync(string slug);
    Task<PollVoteDto> GetPollWithOptionsAsync(string slug);
    Task<IEnumerable<PollSummaryDto>> GetCreatorPollsAsync(int creatorId);
}
