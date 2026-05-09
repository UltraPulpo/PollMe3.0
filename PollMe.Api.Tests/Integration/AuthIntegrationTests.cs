using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PollMe.Api.Dtos;
using PollMe.Api.Hubs;

namespace PollMe.Api.Tests.Integration;

public class AuthIntegrationTests
{
    private (WebApplicationFactory<Program> factory, HttpClient client, string dbPath) CreateFactory()
    {
        var dbPath = Path.GetTempFileName() + ".db";
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b =>
            {
                b.UseSetting("Database:Path", dbPath);
                b.UseSetting("Jwt:Secret", "integration-test-secret-that-is-long-enough");
                b.UseSetting("Jwt:Issuer", "pollme-test");
                b.UseSetting("Jwt:Audience", "pollme-test");
                b.ConfigureTestServices(services =>
                {
                    services.AddSingleton(Substitute.For<IHubContext<TallyHub>>());
                });
            });
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        return (factory, client, dbPath);
    }

    [Fact]
    public async Task Register_ValidRequest_Returns201AndSetsCookie()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            var response = await client.PostAsJsonAsync("/api/auth/register",
                new RegisterRequest { Username = "alice", Password = "Password1!" });

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var setCookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
            setCookieHeader.Should().Contain("jwt=");
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { /* SQLite may still hold the file */ }
        }
    }

    [Fact]
    public async Task Register_DuplicateUsername_Returns409()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            var request = new RegisterRequest { Username = "alice", Password = "Password1!" };
            await client.PostAsJsonAsync("/api/auth/register", request);

            var response = await client.PostAsJsonAsync("/api/auth/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200AndSetsCookie()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await client.PostAsJsonAsync("/api/auth/register",
                new RegisterRequest { Username = "bob", Password = "Password1!" });

            var response = await client.PostAsJsonAsync("/api/auth/login",
                new LoginRequest { Username = "bob", Password = "Password1!" });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var setCookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
            setCookieHeader.Should().Contain("jwt=");
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await client.PostAsJsonAsync("/api/auth/register",
                new RegisterRequest { Username = "carol", Password = "Password1!" });

            var response = await client.PostAsJsonAsync("/api/auth/login",
                new LoginRequest { Username = "carol", Password = "WrongPassword!" });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task Logout_Returns200AndClearsCookie()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await client.PostAsJsonAsync("/api/auth/register",
                new RegisterRequest { Username = "dave", Password = "Password1!" });

            var response = await client.PostAsJsonAsync("/api/auth/logout", null as object);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var setCookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
            setCookieHeader.Should().Contain("max-age=0");
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task GetMe_WithValidCookie_Returns200WithCreatorInfo()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await client.PostAsJsonAsync("/api/auth/register",
                new RegisterRequest { Username = "eve", Password = "Password1!" });

            var response = await client.GetAsync("/api/auth/me");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            body.Should().ContainKey("username");
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task GetMe_WithNoCookie_Returns401()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            var response = await client.GetAsync("/api/auth/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }
}
