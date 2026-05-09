namespace PollMe.Api.Models;
public class Vote
{
    public int Id { get; init; }
    public int PollId { get; init; }
    public string SessionToken { get; init; } = "";
    public DateTime CreatedAt { get; init; }
}
