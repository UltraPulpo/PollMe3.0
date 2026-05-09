using System.ComponentModel.DataAnnotations;
namespace PollMe.Api.Dtos;
public class LoginRequest
{
    [Required] public string Username { get; init; } = "";
    [Required] public string Password { get; init; } = "";
}
