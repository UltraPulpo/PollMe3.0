using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollMe.Api.Dtos;
using PollMe.Api.Infrastructure;
using PollMe.Api.Services;

namespace PollMe.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth, IJwtService jwt, AppConfig config) : ControllerBase
{
    private const string CookieName = "jwt";

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var creator = await auth.RegisterAsync(request.Username, request.Password);
        var token = jwt.IssueToken(creator);
        SetJwtCookie(token);
        return StatusCode(201, new { id = creator.Id, username = creator.Username });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var creator = await auth.LoginAsync(request.Username, request.Password);
        if (creator is null)
            return Unauthorized(new { error = "Invalid credentials" });

        var token = jwt.IssueToken(creator);
        SetJwtCookie(token);
        return Ok(new { id = creator.Id, username = creator.Username });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Append(CookieName, "", BuildCookieOptions(DateTimeOffset.UnixEpoch, TimeSpan.Zero, Request.IsHttps));
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var username = User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
        if (sub is null || username is null)
            return Unauthorized(new { error = "Invalid token claims" });
        return Ok(new { id = int.Parse(sub), username });
    }

    private void SetJwtCookie(string token)
    {
        Response.Cookies.Append(CookieName, token, BuildCookieOptions(DateTimeOffset.UtcNow.AddMinutes(config.Jwt.ExpiryMinutes), secure: Request.IsHttps));
    }

    private static CookieOptions BuildCookieOptions(DateTimeOffset? expires = null, TimeSpan? maxAge = null, bool secure = false)
    {
        return new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = secure,
            SameSite = SameSiteMode.Strict,
            Expires = expires,
            MaxAge = maxAge
        };
    }
}
