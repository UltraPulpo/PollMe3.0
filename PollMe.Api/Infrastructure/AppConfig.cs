namespace PollMe.Api.Infrastructure;

public class AppConfig
{
    public JwtConfig Jwt { get; init; } = new();
    public DatabaseConfig Database { get; init; } = new();
    public PollConfig Poll { get; init; } = new();
    public VoteTokenConfig VoteToken { get; init; } = new();
}

public class JwtConfig
{
    public string Secret { get; init; } = "";
    public string Issuer { get; init; } = "pollme";
    public string Audience { get; init; } = "pollme";
    public int ExpiryMinutes { get; init; } = 1440;
}

public class DatabaseConfig
{
    public string Path { get; init; } = "pollme.db";
}

public class PollConfig
{
    public int MinOptions { get; init; } = 2;
    public int MaxOptions { get; init; } = 10;
}

public class VoteTokenConfig
{
    public int ExpiryDays { get; init; } = 365;
}
