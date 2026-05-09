using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollMe.Api.Dtos;
using PollMe.Api.Services;

namespace PollMe.Api.Controllers;

[ApiController]
[Route("api/polls")]
public class PollsController(IPollService polls) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
    {
        var creatorId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        var poll = await polls.CreatePollAsync(creatorId, request);
        return StatusCode(201, new CreatePollResponseDto
        {
            Slug = poll.Slug,
            VoteLink = $"/vote/{poll.Slug}"
        });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetPolls()
    {
        var creatorId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
        var summaries = await polls.GetCreatorPollsAsync(creatorId);
        return Ok(summaries);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetPoll(string slug)
    {
        var dto = await polls.GetPollWithOptionsAsync(slug);
        return Ok(dto);
    }
}
