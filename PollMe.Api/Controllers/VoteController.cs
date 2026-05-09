using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PollMe.Api.Dtos;
using PollMe.Api.Hubs;
using PollMe.Api.Models;
using PollMe.Api.Services;

namespace PollMe.Api.Controllers;

[ApiController]
[Route("api/polls")]
public class VoteController(
    IPollService pollService,
    IVoteService voteService,
    IHubContext<TallyHub> hub) : ControllerBase
{
    [HttpPost("{slug}/votes")]
    public async Task<IActionResult> Vote(string slug, [FromBody] VoteRequest request)
    {
        var pollDto = await pollService.GetPollWithOptionsAsync(slug);

        var validOptionIds = pollDto.Options.Select(o => o.Id).ToHashSet();
        if (request.SelectedOptionIds.Any(id => !validOptionIds.Contains(id)))
            return BadRequest(new { error = "Invalid option ID" });

        var poll = await pollService.GetBySlugAsync(slug);
        var tally = await voteService.SubmitVoteAsync(poll, request.SelectedOptionIds);

        await hub.Clients.Group($"poll-{poll.Id}").SendAsync("ReceiveTallyUpdate", tally);

        if (poll.Visibility == ResultsVisibility.Public)
            return Ok(tally);

        return Ok(new { message = "Thank you for voting!" });
    }
}
