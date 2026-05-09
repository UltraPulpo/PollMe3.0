using FluentAssertions;
using NSubstitute;
using PollMe.Api.Dtos;
using PollMe.Api.Exceptions;
using PollMe.Api.Models;
using PollMe.Api.Repositories;
using PollMe.Api.Services;

namespace PollMe.Api.Tests.Services;

public class VoteServiceTests
{
    private readonly IVoteRepository _voteRepo = Substitute.For<IVoteRepository>();
    private readonly VoteService _sut;

    public VoteServiceTests()
    {
        _sut = new VoteService(_voteRepo);
        // Default: CreateAsync returns a placeholder Vote
        _voteRepo.CreateAsync(Arg.Any<Vote>(), Arg.Any<IEnumerable<int>>())
                 .Returns(new Vote { Id = 1, PollId = 1 });
    }

    private static Poll SingleSelectPoll(int id = 1) =>
        new() { Id = id, Mode = PollMode.SingleSelect, Question = "Q?", Slug = "abc" };

    private static Poll MultiSelectPoll(int id = 1) =>
        new() { Id = id, Mode = PollMode.MultiSelect, Question = "Q?", Slug = "abc" };

    [Fact]
    public async Task Submit_SingleSelectWithMultipleIds_ThrowsValidation()
    {
        _voteRepo.GetTalliesAsync(Arg.Any<int>()).Returns(Array.Empty<OptionTally>());

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.SubmitVoteAsync(SingleSelectPoll(), [1, 2]));
    }

    [Fact]
    public async Task Submit_MultiSelectWithNoIds_ThrowsValidation()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.SubmitVoteAsync(MultiSelectPoll(), []));
    }

    [Fact]
    public async Task Submit_ValidSingleSelect_CallsRepoAndReturnsTally()
    {
        _voteRepo.GetTalliesAsync(1).Returns(new[]
        {
            new OptionTally { OptionId = 1, Text = "A", Votes = 1 }
        });

        var tally = await _sut.SubmitVoteAsync(SingleSelectPoll(), [1]);

        await _voteRepo.Received(1).CreateAsync(Arg.Any<Vote>(), Arg.Is<IEnumerable<int>>(ids => ids.Contains(1)));
        tally.Options.Should().HaveCount(1);
    }

    [Fact]
    public async Task Submit_DuplicateSelections_ThrowsValidation()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.SubmitVoteAsync(MultiSelectPoll(), [1, 1]));
    }

    [Fact]
    public async Task Submit_ValidMultiSelect_PersistsAllSelections()
    {
        _voteRepo.GetTalliesAsync(1).Returns(new[]
        {
            new OptionTally { OptionId = 1, Text = "A", Votes = 1 },
            new OptionTally { OptionId = 2, Text = "B", Votes = 1 },
            new OptionTally { OptionId = 3, Text = "C", Votes = 1 }
        });

        await _sut.SubmitVoteAsync(MultiSelectPoll(), [1, 2, 3]);

        await _voteRepo.Received(1).CreateAsync(
            Arg.Any<Vote>(),
            Arg.Is<IEnumerable<int>>(ids => ids.Count() == 3 && ids.Contains(1) && ids.Contains(2) && ids.Contains(3)));
    }

    [Fact]
    public async Task GetTallies_CorrectlyComputesRoundedPercentages()
    {
        // 1 vote out of 3 total → Math.Round(1/3.0 * 100) = 33
        _voteRepo.GetTalliesAsync(1).Returns(new[]
        {
            new OptionTally { OptionId = 1, Text = "A", Votes = 1 },
            new OptionTally { OptionId = 2, Text = "B", Votes = 2 }
        });

        var tally = await _sut.GetTalliesAsync(1);

        tally.Options.First(o => o.OptionId == 1).Percentage.Should().Be(33);
        tally.Options.First(o => o.OptionId == 2).Percentage.Should().Be(67);
    }

    [Fact]
    public async Task GetTallies_NoVotesCast_AllOptionsReturnZeroPercent()
    {
        _voteRepo.GetTalliesAsync(1).Returns(new[]
        {
            new OptionTally { OptionId = 1, Text = "A", Votes = 0 },
            new OptionTally { OptionId = 2, Text = "B", Votes = 0 }
        });

        var tally = await _sut.GetTalliesAsync(1);

        tally.Options.Should().AllSatisfy(o => o.Percentage.Should().Be(0));
    }
}
