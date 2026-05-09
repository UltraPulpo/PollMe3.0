using System.ComponentModel.DataAnnotations;
namespace PollMe.Api.Dtos;
public class RegisterRequest
{
    [Required] public string Username { get; init; } = "";
    [Required] public string Password { get; init; } = "";
}
