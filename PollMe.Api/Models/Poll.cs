namespace PollMe.Api.Models;
public class Poll
{
    public int Id { get; init; }
    public int CreatorId { get; init; }
    public string Question { get; init; } = "";
    public string Slug { get; init; } = "";
    public PollMode Mode { get; init; }
    public ResultsVisibility Visibility { get; init; }
    public DateTime CreatedAt { get; init; }
}
