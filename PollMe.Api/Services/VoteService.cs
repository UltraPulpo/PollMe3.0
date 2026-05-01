using PollMe.Api.Dtos;
using PollMe.Api.Exceptions;
using PollMe.Api.Models;
using PollMe.Api.Repositories;

namespace PollMe.Api.Services;

public class VoteService(IVoteRepository voteRepo) : IVoteService
{
    public async Task<TallyDto> SubmitVoteAsync(Poll poll, IEnumerable<int> selectedOptionIds)
    {
        var ids = selectedOptionIds.ToList();

        if (poll.Mode == PollMode.SingleSelect)
        {
            if (ids.Count != 1)
                throw new ValidationException("Single-select polls require exactly one selection");
        }
        else
        {
            if (ids.Count == 0)
                throw new ValidationException("At least one option must be selected");
        }

        await voteRepo.CreateAsync(new Vote { PollId = poll.Id, CreatedAt = DateTime.UtcNow }, ids);

        return await GetTalliesAsync(poll.Id);
    }

    public async Task<TallyDto> GetTalliesAsync(int pollId)
    {
        var tallies = (await voteRepo.GetTalliesAsync(pollId)).ToList();
        var total = tallies.Sum(t => t.Votes);

        var options = tallies.Select(t => new OptionTallyDto
        {
            OptionId = t.OptionId,
            Text = t.Text,
            Votes = t.Votes,
            Percentage = total == 0 ? 0 : (int)Math.Round((double)t.Votes / total * 100)
        }).ToArray();

        return new TallyDto { PollId = pollId, Options = options };
    }
}
