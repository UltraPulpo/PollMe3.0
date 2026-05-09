namespace PollMe.Api.Models;
public class OptionTally
{
    public int OptionId { get; init; }
    public string Text { get; init; } = "";
    public int Votes { get; init; }
}
