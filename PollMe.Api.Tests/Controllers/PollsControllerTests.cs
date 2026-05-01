using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PollMe.Api.Controllers;
using PollMe.Api.Dtos;
using PollMe.Api.Exceptions;
using PollMe.Api.Models;
using PollMe.Api.Services;

namespace PollMe.Api.Tests.Controllers;

public class PollsControllerTests
{
    private readonly IPollService _pollService = Substitute.For<IPollService>();
    private readonly PollsController _sut;

    public PollsControllerTests()
    {
        _sut = new PollsController(_pollService);
    }

    private void SetAuthenticatedUser(int creatorId = 1, string username = "alice")
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, creatorId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username)
        };
        var identity = new ClaimsIdentity(claims, "test");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task Create_Authenticated_Returns201WithSlug()
    {
        SetAuthenticatedUser();
        var request = new CreatePollRequest
        {
            Question = "Q?", Options = ["A", "B"],
            Mode = PollMode.SingleSelect, Visibility = ResultsVisibility.Public
        };
        _pollService.CreatePollAsync(1, request)
                    .Returns(new Poll { Id = 1, Slug = "abc123", Question = "Q?" });

        var result = await _sut.CreatePoll(request);

        var obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(201);
        var dto = obj.Value.Should().BeOfType<CreatePollResponseDto>().Subject;
        dto.Slug.Should().Be("abc123");
    }

    [Fact]
    public async Task Create_ServiceThrowsValidation_Returns400ViaPropagation()
    {
        SetAuthenticatedUser();
        var request = new CreatePollRequest
        {
            Question = "Q?", Options = ["A"],
            Mode = PollMode.SingleSelect, Visibility = ResultsVisibility.Public
        };
        _pollService.CreatePollAsync(Arg.Any<int>(), Arg.Any<CreatePollRequest>())
                    .ThrowsAsync(new ValidationException("Too few options"));

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreatePoll(request));
    }

    [Fact]
    public async Task GetMyPolls_Authenticated_ReturnsList()
    {
        SetAuthenticatedUser();
        var summaries = new List<PollSummaryDto>
        {
            new() { Id = 1, Slug = "abc", Question = "Q?", TotalVotes = 5 }
        };
        _pollService.GetCreatorPollsAsync(1).Returns(summaries);

        var result = await _sut.GetPolls();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(summaries);
    }

    [Fact]
    public async Task GetMyPolls_Unauthenticated_ThrowsWhenSubClaimMissing()
    {
        // Without [Authorize] middleware, unauthenticated users hit the code that reads Sub claim
        // which throws NullReferenceException - this verifies the dependency on auth middleware
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        await Assert.ThrowsAsync<NullReferenceException>(() => _sut.GetPolls());
    }

    [Fact]
    public async Task GetBySlug_Exists_Returns200WithPollVoteDto()
    {
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        var dto = new PollVoteDto { Id = 1, Slug = "abc", Question = "Q?", Mode = PollMode.SingleSelect };
        _pollService.GetPollWithOptionsAsync("abc").Returns(dto);

        var result = await _sut.GetPoll("abc");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetBySlug_ServiceThrowsNotFound_PropagatesException()
    {
        _sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _pollService.GetPollWithOptionsAsync("nope").ThrowsAsync(new NotFoundException("Poll not found"));

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetPoll("nope"));
    }
}
