using PollMe.Api.Dtos;
using PollMe.Api.Exceptions;
using PollMe.Api.Infrastructure;
using PollMe.Api.Models;
using PollMe.Api.Repositories;

namespace PollMe.Api.Services;

public class PollService(
    IPollRepository pollRepo,
    IVoteRepository voteRepo,
    ISlugService slugService,
    AppConfig config) : IPollService
{
    public async Task<Poll> CreatePollAsync(int creatorId, CreatePollRequest request)
    {
        if (request.Options.Length < config.Poll.MinOptions || request.Options.Length > config.Poll.MaxOptions)
            throw new ValidationException($"Poll must have between {config.Poll.MinOptions} and {config.Poll.MaxOptions} options");

        var slug = await slugService.GenerateUniqueSlugAsync();

        var poll = new Poll
        {
            CreatorId = creatorId,
            Question = request.Question,
            Slug = slug,
            Mode = request.Mode,
            Visibility = request.Visibility,
            CreatedAt = DateTime.UtcNow
        };

        var options = request.Options.Select((text, index) => new Option
        {
            Text = text,
            Position = index
        });

        return await pollRepo.CreateWithOptionsAsync(poll, options);
    }

    public async Task<Poll> GetBySlugAsync(string slug)
    {
        var poll = await pollRepo.GetBySlugAsync(slug);
        if (poll is null)
            throw new NotFoundException("Poll not found");
        return poll;
    }

    public async Task<PollVoteDto> GetPollWithOptionsAsync(string slug)
    {
        var poll = await GetBySlugAsync(slug);
        var options = await pollRepo.GetOptionsByPollIdAsync(poll.Id);

        return new PollVoteDto
        {
            Id = poll.Id,
            Slug = poll.Slug,
            Question = poll.Question,
            Mode = poll.Mode,
            Options = options.Select(o => new OptionDto
            {
                Id = o.Id,
                Text = o.Text,
                Position = o.Position
            }).ToArray()
        };
    }

    public async Task<IEnumerable<PollSummaryDto>> GetCreatorPollsAsync(int creatorId)
    {
        var polls = await pollRepo.GetByCreatorIdAsync(creatorId);
        var summaries = new List<PollSummaryDto>();

        foreach (var poll in polls)
        {
            var tallies = (await voteRepo.GetTalliesAsync(poll.Id)).ToList();
            var totalVotes = tallies.Sum(t => t.Votes);

            OptionTally? leading = null;
            if (totalVotes > 0)
                leading = tallies.OrderByDescending(t => t.Votes).First();

            var leadingPct = totalVotes == 0 ? 0
                : (int)Math.Round((double)(leading!.Votes) / totalVotes * 100);

            summaries.Add(new PollSummaryDto
            {
                Id = poll.Id,
                Slug = poll.Slug,
                Question = poll.Question,
                TotalVotes = totalVotes,
                LeadingOptionText = leading?.Text,
                LeadingPercentage = leadingPct,
                CreatedAt = poll.CreatedAt
            });
        }

        return summaries;
    }
}
