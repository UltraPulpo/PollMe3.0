using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using PollMe.Api.Infrastructure;
using PollMe.Api.Models;
using PollMe.Api.Services;

namespace PollMe.Api.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _sut;
    private readonly Creator _creator = new() { Id = 42, Username = "alice" };

    public JwtServiceTests()
    {
        var config = new AppConfig
        {
            Jwt = new JwtConfig
            {
                Secret = "this-is-a-super-secret-key-for-testing-purposes",
                Issuer = "pollme",
                Audience = "pollme",
                ExpiryMinutes = 60
            }
        };
        _sut = new JwtService(config);
    }

    [Fact]
    public void IssueToken_ValidCreator_ReturnsSignedJwtWithSubAndUsernameClaims()
    {
        var token = _sut.IssueToken(_creator);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be("42");
        jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value.Should().Be("alice");
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
    }
}
