namespace PollMe.Api.Services;

public interface ISlugService
{
    Task<string> GenerateUniqueSlugAsync();
}
