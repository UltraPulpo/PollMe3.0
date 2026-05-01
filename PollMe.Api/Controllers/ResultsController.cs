using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using PollMe.Api.Models;
using PollMe.Api.Services;

namespace PollMe.Api.Controllers;

[ApiController]
[Route("api/polls")]
public class ResultsController(IPollService pollService, IVoteService voteService) : ControllerBase
{
    [HttpGet("{slug}/results")]
    public async Task<IActionResult> GetResults(string slug)
    {
        var poll = await pollService.GetBySlugAsync(slug);

        if (poll.Visibility == ResultsVisibility.CreatorOnly)
        {
            if (User.Identity?.IsAuthenticated != true)
                return Forbid();

            var callerId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);
            if (callerId != poll.CreatorId)
                return Forbid();
        }

        var tally = await voteService.GetTalliesAsync(poll.Id);
        return Ok(tally);
    }
}
