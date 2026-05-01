using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PollMe.Api.Controllers;
using PollMe.Api.Dtos;
using PollMe.Api.Exceptions;
using PollMe.Api.Infrastructure;
using PollMe.Api.Models;
using PollMe.Api.Services;

namespace PollMe.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly IAuthService _auth = Substitute.For<IAuthService>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly AppConfig _config = new() { Jwt = new JwtConfig { ExpiryMinutes = 60 } };
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_auth, _jwt, _config);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _jwt.IssueToken(Arg.Any<Creator>()).Returns("test-token");
    }

    [Fact]
    public async Task Register_ValidRequest_Returns201WithHttpOnlyCookie()
    {
        _auth.RegisterAsync("alice", "Password1!")
             .Returns(new Creator { Id = 1, Username = "alice" });

        var result = await _sut.Register(new RegisterRequest { Username = "alice", Password = "Password1!" });

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(201);

        var setCookie = _sut.Response.Headers["Set-Cookie"].ToString();
        setCookie.Should().Contain("jwt=");
        setCookie.ToLowerInvariant().Should().Contain("httponly");
    }

    [Fact]
    public async Task Register_DuplicateUsername_ThrowsConflict()
    {
        _auth.RegisterAsync("alice", "Password1!")
             .ThrowsAsync(new ConflictException("Username already taken"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.Register(new RegisterRequest { Username = "alice", Password = "Password1!" }));
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithCookie()
    {
        _auth.LoginAsync("alice", "Password1!")
             .Returns(new Creator { Id = 1, Username = "alice" });

        var result = await _sut.Login(new LoginRequest { Username = "alice", Password = "Password1!" });

        result.Should().BeOfType<OkObjectResult>();
        var setCookie = _sut.Response.Headers["Set-Cookie"].ToString();
        setCookie.Should().Contain("jwt=");
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        _auth.LoginAsync("alice", "wrong").Returns((Creator?)null);

        var result = await _sut.Login(new LoginRequest { Username = "alice", Password = "wrong" });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void Logout_ClearsCookieWithZeroMaxAge()
    {
        var result = _sut.Logout();

        result.Should().BeOfType<OkResult>();
        var setCookie = _sut.Response.Headers["Set-Cookie"].ToString();
        setCookie.Should().Contain("jwt=");
        setCookie.ToLowerInvariant().Should().Contain("max-age=0");
    }
}
