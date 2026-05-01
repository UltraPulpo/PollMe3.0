namespace PollMe.Api.Dtos;
public class OptionTallyDto
{
    public int OptionId { get; init; }
    public string Text { get; init; } = "";
    public int Votes { get; init; }
    public int Percentage { get; init; }
}
