using PollMe.Api.Repositories;

namespace PollMe.Api.Services;

public class SlugService(IPollRepository pollRepo) : ISlugService
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public async Task<string> GenerateUniqueSlugAsync()
    {
        for (int i = 0; i < 5; i++)
        {
            var candidate = new string(Enumerable.Range(0, 6)
                .Select(_ => Alphabet[Random.Shared.Next(Alphabet.Length)])
                .ToArray());

            if (!await pollRepo.SlugExistsAsync(candidate))
                return candidate;
        }

        throw new Exception("Failed to generate a unique slug after 5 attempts");
    }
}
