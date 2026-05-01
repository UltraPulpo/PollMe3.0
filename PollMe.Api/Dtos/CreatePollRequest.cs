using PollMe.Api.Models;
namespace PollMe.Api.Dtos;
public class CreatePollRequest
{
    public string Question { get; init; } = "";
    public string[] Options { get; init; } = [];
    public PollMode Mode { get; init; }
    public ResultsVisibility Visibility { get; init; }
}
