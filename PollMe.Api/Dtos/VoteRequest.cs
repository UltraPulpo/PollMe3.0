namespace PollMe.Api.Dtos;
public class VoteRequest
{
    public int[] SelectedOptionIds { get; init; } = [];
}
