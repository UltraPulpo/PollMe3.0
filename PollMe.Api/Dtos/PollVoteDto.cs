using PollMe.Api.Models;
namespace PollMe.Api.Dtos;
public class PollVoteDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = "";
    public string Question { get; init; } = "";
    public OptionDto[] Options { get; init; } = [];
    public PollMode Mode { get; init; }
}
