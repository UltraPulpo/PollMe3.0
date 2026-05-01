using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PollMe.Api.Infrastructure;
using PollMe.Api.Models;

namespace PollMe.Api.Services;

public class JwtService(AppConfig config) : IJwtService
{
    public string IssueToken(Creator creator)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, creator.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, creator.Username)
        };

        var token = new JwtSecurityToken(
            issuer: config.Jwt.Issuer,
            audience: config.Jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(config.Jwt.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
