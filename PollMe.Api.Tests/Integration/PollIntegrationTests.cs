using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PollMe.Api.Dtos;
using PollMe.Api.Hubs;
using PollMe.Api.Models;
using PollMe.Api.Tests.Helpers;

namespace PollMe.Api.Tests.Integration;

public class PollIntegrationTests
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
    public async Task CreatePoll_AuthenticatedValidRequest_PersistsPollAndOptions()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);

            var request = new PollBuilder().Build();
            var response = await client.PostAsJsonAsync("/api/polls", request);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var body = await response.Content.ReadFromJsonAsync<CreatePollResponseDto>();
            body.Should().NotBeNull();
            body!.Slug.Should().NotBeNullOrEmpty();
            body.VoteLink.Should().NotBeNullOrEmpty();
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task CreatePoll_BelowMinOptions_Returns400()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);

            var request = new PollBuilder().WithOptions("Only One Option").Build();
            var response = await client.PostAsJsonAsync("/api/polls", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task CreatePoll_Unauthenticated_Returns401()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            var request = new PollBuilder().Build();
            var response = await client.PostAsJsonAsync("/api/polls", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task GetMyPolls_Returns_OnlyCallerPolls()
    {
        var (factory, client1, dbPath) = CreateFactory();
        try
        {
            var client2 = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

            await AuthHelper.AuthenticateAsync(client1, "user1");
            await AuthHelper.AuthenticateAsync(client2, "user2");

            var create1Response = await client1.PostAsJsonAsync("/api/polls", new PollBuilder().Build());
            create1Response.StatusCode.Should().Be(HttpStatusCode.Created);
            var poll1 = await create1Response.Content.ReadFromJsonAsync<CreatePollResponseDto>();

            var create2Response = await client2.PostAsJsonAsync("/api/polls", new PollBuilder().Build());
            create2Response.StatusCode.Should().Be(HttpStatusCode.Created);
            var poll2 = await create2Response.Content.ReadFromJsonAsync<CreatePollResponseDto>();

            var user1Polls = await client1.GetFromJsonAsync<PollSummaryDto[]>("/api/polls");
            var user2Polls = await client2.GetFromJsonAsync<PollSummaryDto[]>("/api/polls");

            user1Polls.Should().HaveCount(1);
            user1Polls![0].Slug.Should().Be(poll1!.Slug);

            user2Polls.Should().HaveCount(1);
            user2Polls![0].Slug.Should().Be(poll2!.Slug);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task GetMyPolls_IncludesVoteCountPerPoll()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);

            var createResponse = await client.PostAsJsonAsync("/api/polls", new PollBuilder().Build());
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResponse.Content.ReadFromJsonAsync<CreatePollResponseDto>();
            var slug = created!.Slug;

            await using var conn = new SqliteConnection($"Data Source={dbPath}");
            conn.Open();

            var pollId = await conn.ExecuteScalarAsync<int>(
                "SELECT id FROM polls WHERE slug = @Slug", new { Slug = slug });
            var optionId = await conn.ExecuteScalarAsync<int>(
                "SELECT id FROM options WHERE poll_id = @PollId LIMIT 1", new { PollId = pollId });

            await DbSeedHelper.InsertVoteAsync(conn, pollId, optionId);

            var polls = await client.GetFromJsonAsync<PollSummaryDto[]>("/api/polls");
            var poll = polls!.Single(p => p.Slug == slug);
            poll.TotalVotes.Should().Be(1);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task GetPollBySlug_Exists_Returns200WithOptions()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);

            var createResponse = await client.PostAsJsonAsync("/api/polls", new PollBuilder().Build());
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResponse.Content.ReadFromJsonAsync<CreatePollResponseDto>();
            var slug = created!.Slug;

            var response = await client.GetAsync($"/api/polls/{slug}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await response.Content.ReadFromJsonAsync<PollVoteDto>();
            dto.Should().NotBeNull();
            dto!.Question.Should().NotBeNullOrEmpty();
            dto.Options.Should().HaveCountGreaterThanOrEqualTo(2);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task GetPollBySlug_NonExistent_Returns404()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            var response = await client.GetAsync("/api/polls/nonexistent-slug");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }
}

