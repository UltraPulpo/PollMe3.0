namespace PollMe.Api.Dtos;
public class TallyDto
{
    public int PollId { get; init; }
    public OptionTallyDto[] Options { get; init; } = [];
}
