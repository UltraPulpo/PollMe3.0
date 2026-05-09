using System.Net.Http.Json;
using PollMe.Api.Dtos;

namespace PollMe.Api.Tests.Helpers;

public static class AuthHelper
{
    public static async Task<HttpClient> AuthenticateAsync(
        HttpClient client,
        string username = "testuser",
        string password = "Password1!")
    {
        var request = new RegisterRequest { Username = username, Password = password };
        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        return client;
    }
}
