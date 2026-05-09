using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PollMe.Api.Controllers;
using PollMe.Api.Dtos;
using PollMe.Api.Exceptions;
using PollMe.Api.Hubs;
using PollMe.Api.Models;
using PollMe.Api.Services;

namespace PollMe.Api.Tests.Controllers;

public class VoteControllerTests
{
    private readonly IPollService _pollService = Substitute.For<IPollService>();
    private readonly IVoteService _voteService = Substitute.For<IVoteService>();
    private readonly IHubContext<TallyHub> _hub = Substitute.For<IHubContext<TallyHub>>();
    private readonly IClientProxy _group = Substitute.For<IClientProxy>();
    private readonly VoteController _sut;

    public VoteControllerTests()
    {
        var clients = Substitute.For<IHubClients>();
        clients.Group(Arg.Any<string>()).Returns(_group);
        _hub.Clients.Returns(clients);
        _sut = new VoteController(_pollService, _voteService, _hub);
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    }

    private static Poll PublicPoll(int id = 1) =>
        new() { Id = id, Slug = "abc", Question = "Q?", Mode = PollMode.SingleSelect, Visibility = ResultsVisibility.Public };

    private static Poll CreatorOnlyPoll(int creatorId = 1) =>
        new() { Id = 1, Slug = "abc", Question = "Q?", CreatorId = creatorId, Mode = PollMode.SingleSelect, Visibility = ResultsVisibility.CreatorOnly };

    private static PollVoteDto PollVoteDtoWithOptions(params int[] optionIds) =>
        new()
        {
            Id = 1, Slug = "abc", Question = "Q?", Mode = PollMode.SingleSelect,
            Options = optionIds.Select((id, i) => new OptionDto { Id = id, Text = $"Option {i}", Position = i }).ToArray()
        };

    private static TallyDto EmptyTally(int pollId = 1) =>
        new() { PollId = pollId, Options = [] };

    [Fact]
    public async Task Submit_PublicPoll_Returns200WithTally()
    {
        var tally = new TallyDto { PollId = 1, Options = [new OptionTallyDto { OptionId = 1, Text = "A", Votes = 1, Percentage = 100 }] };
        _pollService.GetPollWithOptionsAsync("abc").Returns(PollVoteDtoWithOptions(1));
        _pollService.GetBySlugAsync("abc").Returns(PublicPoll());
        _voteService.SubmitVoteAsync(Arg.Any<Poll>(), Arg.Any<IEnumerable<int>>()).Returns(tally);

        var result = await _sut.Vote("abc", new VoteRequest { SelectedOptionIds = [1] });

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(tally);
    }

    [Fact]
    public async Task Submit_CreatorOnlyPoll_Returns200WithThankYouMessage()
    {
        _pollService.GetPollWithOptionsAsync("abc").Returns(PollVoteDtoWithOptions(1));
        _pollService.GetBySlugAsync("abc").Returns(CreatorOnlyPoll());
        _voteService.SubmitVoteAsync(Arg.Any<Poll>(), Arg.Any<IEnumerable<int>>()).Returns(EmptyTally());

        var result = await _sut.Vote("abc", new VoteRequest { SelectedOptionIds = [1] });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new { message = "Thank you for voting!" });
    }

    [Fact]
    public async Task Submit_UnknownOptionId_Returns400()
    {
        _pollService.GetPollWithOptionsAsync("abc").Returns(PollVoteDtoWithOptions(1, 2));

        var result = await _sut.Vote("abc", new VoteRequest { SelectedOptionIds = [999] });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Submit_ServiceThrowsValidation_PropagatesException()
    {
        _pollService.GetPollWithOptionsAsync("abc").Returns(PollVoteDtoWithOptions(1));
        _pollService.GetBySlugAsync("abc").Returns(PublicPoll());
        _voteService.SubmitVoteAsync(Arg.Any<Poll>(), Arg.Any<IEnumerable<int>>())
                    .ThrowsAsync(new ValidationException("Single-select polls require exactly one selection"));

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.Vote("abc", new VoteRequest { SelectedOptionIds = [1] }));
    }

    [Fact]
    public async Task Submit_ValidVote_BroadcastsTallyToSignalRGroup()
    {
        var poll = PublicPoll(7);
        _pollService.GetPollWithOptionsAsync("abc").Returns(PollVoteDtoWithOptions(1));
        _pollService.GetBySlugAsync("abc").Returns(poll);
        _voteService.SubmitVoteAsync(Arg.Any<Poll>(), Arg.Any<IEnumerable<int>>()).Returns(EmptyTally(7));

        await _sut.Vote("abc", new VoteRequest { SelectedOptionIds = [1] });

        _hub.Clients.Received().Group("poll-7");
        await _group.Received().SendCoreAsync(
            "ReceiveTallyUpdate",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }
}
