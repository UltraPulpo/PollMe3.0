using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PollMe.Api.Controllers;
using PollMe.Api.Dtos;
using PollMe.Api.Models;
using PollMe.Api.Services;

namespace PollMe.Api.Tests.Controllers;

public class ResultsControllerTests
{
    private readonly IPollService _pollService = Substitute.For<IPollService>();
    private readonly IVoteService _voteService = Substitute.For<IVoteService>();
    private readonly ResultsController _sut;

    public ResultsControllerTests()
    {
        _sut = new ResultsController(_pollService, _voteService);
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _voteService.GetTalliesAsync(Arg.Any<int>()).Returns(new TallyDto { PollId = 1, Options = [] });
    }

    private void SetAuthenticatedUser(int creatorId)
    {
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, creatorId.ToString()) };
        var identity = new ClaimsIdentity(claims, "test");
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [Fact]
    public async Task GetResults_PublicPoll_Returns200()
    {
        _pollService.GetBySlugAsync("abc").Returns(new Poll { Id = 1, Visibility = ResultsVisibility.Public, Slug = "abc", Question = "Q?" });

        var result = await _sut.GetResults("abc");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetResults_CreatorOnlyUnauthenticated_Returns403()
    {
        _pollService.GetBySlugAsync("abc").Returns(new Poll { Id = 1, CreatorId = 1, Visibility = ResultsVisibility.CreatorOnly, Slug = "abc", Question = "Q?" });

        var result = await _sut.GetResults("abc");

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetResults_CreatorOnlyByOwner_Returns200()
    {
        SetAuthenticatedUser(creatorId: 1);
        _pollService.GetBySlugAsync("abc").Returns(new Poll { Id = 1, CreatorId = 1, Visibility = ResultsVisibility.CreatorOnly, Slug = "abc", Question = "Q?" });

        var result = await _sut.GetResults("abc");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetResults_CreatorOnlyByNonOwner_Returns403()
    {
        SetAuthenticatedUser(creatorId: 2);
        _pollService.GetBySlugAsync("abc").Returns(new Poll { Id = 1, CreatorId = 1, Visibility = ResultsVisibility.CreatorOnly, Slug = "abc", Question = "Q?" });

        var result = await _sut.GetResults("abc");

        result.Should().BeOfType<ForbidResult>();
    }
}
