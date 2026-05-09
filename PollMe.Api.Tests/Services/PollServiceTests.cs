using FluentAssertions;
using NSubstitute;
using PollMe.Api.Dtos;
using PollMe.Api.Exceptions;
using PollMe.Api.Infrastructure;
using PollMe.Api.Models;
using PollMe.Api.Repositories;
using PollMe.Api.Services;

namespace PollMe.Api.Tests.Services;

public class PollServiceTests
{
    private readonly IPollRepository _pollRepo = Substitute.For<IPollRepository>();
    private readonly IVoteRepository _voteRepo = Substitute.For<IVoteRepository>();
    private readonly ISlugService _slugService = Substitute.For<ISlugService>();
    private readonly AppConfig _config = new()
    {
        Poll = new PollConfig { MinOptions = 2, MaxOptions = 10 }
    };
    private readonly PollService _sut;

    public PollServiceTests()
    {
        _sut = new PollService(_pollRepo, _voteRepo, _slugService, _config);
        _slugService.GenerateUniqueSlugAsync().Returns("abc123");
    }

    [Fact]
    public async Task Create_ValidInput_ReturnsCreatedPollWithSlug()
    {
        var request = new CreatePollRequest
        {
            Question = "Q?",
            Options = ["A", "B"],
            Mode = PollMode.SingleSelect,
            Visibility = ResultsVisibility.Public
        };
        var expectedPoll = new Poll { Id = 1, Slug = "abc123", Question = "Q?" };
        _pollRepo.CreateWithOptionsAsync(Arg.Any<Poll>(), Arg.Any<IEnumerable<Option>>())
                 .Returns(expectedPoll);

        var result = await _sut.CreatePollAsync(1, request);

        result.Slug.Should().Be("abc123");
    }

    [Fact]
    public async Task Create_BelowMinOptions_ThrowsValidation()
    {
        var request = new CreatePollRequest { Question = "Q?", Options = ["A"], Mode = PollMode.SingleSelect, Visibility = ResultsVisibility.Public };

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreatePollAsync(1, request));
    }

    [Fact]
    public async Task Create_AboveMaxOptions_ThrowsValidation()
    {
        var options = Enumerable.Range(1, 11).Select(i => $"Option {i}").ToArray();
        var request = new CreatePollRequest { Question = "Q?", Options = options, Mode = PollMode.SingleSelect, Visibility = ResultsVisibility.Public };

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreatePollAsync(1, request));
    }

    [Fact]
    public async Task GetCreatorPolls_TieForLeading_ReturnsNonNullLeadingOption()
    {
        var poll = new Poll { Id = 1, Slug = "abc", Question = "Q?", CreatedAt = DateTime.UtcNow };
        _pollRepo.GetByCreatorIdAsync(1).Returns(new[] { poll });
        _voteRepo.GetTalliesAsync(1).Returns(new[]
        {
            new OptionTally { OptionId = 1, Text = "Option A", Votes = 2 },
            new OptionTally { OptionId = 2, Text = "Option B", Votes = 2 }
        });

        var summaries = (await _sut.GetCreatorPollsAsync(1)).ToList();

        summaries.Should().HaveCount(1);
        summaries[0].LeadingOptionText.Should().NotBeNullOrEmpty();
        summaries[0].TotalVotes.Should().Be(4);
    }

    [Fact]
    public async Task GetCreatorPolls_NoVotes_ReturnsNullLeadingOptionAndZeroPercent()
    {
        var poll = new Poll { Id = 1, Slug = "abc", Question = "Q?", CreatedAt = DateTime.UtcNow };
        _pollRepo.GetByCreatorIdAsync(1).Returns(new[] { poll });
        _voteRepo.GetTalliesAsync(1).Returns(new[]
        {
            new OptionTally { OptionId = 1, Text = "Option A", Votes = 0 },
            new OptionTally { OptionId = 2, Text = "Option B", Votes = 0 }
        });

        var summaries = (await _sut.GetCreatorPollsAsync(1)).ToList();

        summaries[0].LeadingOptionText.Should().BeNull();
        summaries[0].LeadingPercentage.Should().Be(0);
        summaries[0].TotalVotes.Should().Be(0);
    }
}
