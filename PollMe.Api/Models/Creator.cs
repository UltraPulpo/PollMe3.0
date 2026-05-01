namespace PollMe.Api.Models;
public class Creator
{
    public int Id { get; init; }
    public string Username { get; init; } = "";
    public string PasswordHash { get; init; } = "";
    public DateTime CreatedAt { get; init; }
}
