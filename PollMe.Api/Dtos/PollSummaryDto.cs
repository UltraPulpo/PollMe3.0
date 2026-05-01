namespace PollMe.Api.Dtos;
public class PollSummaryDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = "";
    public string Question { get; init; } = "";
    public int TotalVotes { get; init; }
    public string? LeadingOptionText { get; init; }
    public int LeadingPercentage { get; init; }
    public DateTime CreatedAt { get; init; }
}
