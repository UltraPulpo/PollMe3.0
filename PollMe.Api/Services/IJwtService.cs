using PollMe.Api.Models;

namespace PollMe.Api.Services;

public interface IJwtService
{
    string IssueToken(Creator creator);
}
