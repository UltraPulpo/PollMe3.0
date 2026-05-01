using BCrypt.Net;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ClearExtensions;
using PollMe.Api.Exceptions;
using PollMe.Api.Models;
using PollMe.Api.Repositories;
using PollMe.Api.Services;

namespace PollMe.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly ICreatorRepository _repo = Substitute.For<ICreatorRepository>();
    private readonly AuthService _sut;

    public AuthServiceTests() => _sut = new AuthService(_repo);

    [Fact]
    public async Task Register_NewUsername_StoresPasswordAsBcryptHash()
    {
        _repo.FindByUsernameAsync("alice").Returns((Creator?)null);
        string? capturedHash = null;
        _repo.CreateAsync("alice", Arg.Do<string>(h => capturedHash = h))
             .Returns(ci => new Creator { Id = 1, Username = "alice", PasswordHash = capturedHash! });

        await _sut.RegisterAsync("alice", "Password1!");

        BCrypt.Net.BCrypt.Verify("Password1!", capturedHash).Should().BeTrue();
    }

    [Fact]
    public async Task Register_ExistingUsername_ThrowsConflict()
    {
        _repo.FindByUsernameAsync("alice").Returns(new Creator { Username = "alice" });

        await Assert.ThrowsAsync<ConflictException>(() => _sut.RegisterAsync("alice", "any"));
    }

    [Fact]
    public async Task Login_CorrectPassword_ReturnsCreator()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Password1!");
        var creator = new Creator { Id = 1, Username = "alice", PasswordHash = hash };
        _repo.FindByUsernameAsync("alice").Returns(creator);

        var result = await _sut.LoginAsync("alice", "Password1!");

        result.Should().NotBeNull();
        result!.Username.Should().Be("alice");
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsNull()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correct");
        var creator = new Creator { Id = 1, Username = "alice", PasswordHash = hash };
        _repo.FindByUsernameAsync("alice").Returns(creator);

        var result = await _sut.LoginAsync("alice", "wrong");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_UnknownUsername_ReturnsNull()
    {
        _repo.FindByUsernameAsync("unknown").Returns((Creator?)null);

        var result = await _sut.LoginAsync("unknown", "any");

        result.Should().BeNull();
    }
}
