namespace PollMe.Api.Models;
public class Option
{
    public int Id { get; init; }
    public int PollId { get; init; }
    public string Text { get; init; } = "";
    public int Position { get; init; }
}
