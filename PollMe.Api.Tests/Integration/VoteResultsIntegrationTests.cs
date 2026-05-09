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
using PollMe.Api.Models;
using PollMe.Api.Tests.Helpers;

namespace PollMe.Api.Tests.Integration;

public class VoteResultsIntegrationTests
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

    private async Task<string> CreatePollAndGetSlug(
        HttpClient client,
        PollMode mode = PollMode.SingleSelect,
        ResultsVisibility visibility = ResultsVisibility.Public)
    {
        var req = new PollBuilder()
            .WithMode(mode)
            .WithVisibility(visibility)
            .WithOptions("Option A", "Option B", "Option C")
            .Build();
        var resp = await client.PostAsJsonAsync("/api/polls", req);
        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<CreatePollResponseDto>();
        return created!.Slug;
    }

    private async Task<int[]> GetOptionIds(HttpClient client, string slug)
    {
        var resp = await client.GetFromJsonAsync<PollVoteDto>($"/api/polls/{slug}");
        return resp!.Options.Select(o => o.Id).ToArray();
    }

    [Fact]
    public async Task SubmitVote_ValidSingleSelect_Returns200WithTally()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);
            var slug = await CreatePollAndGetSlug(client);
            var optionIds = await GetOptionIds(client, slug);

            var response = await client.PostAsJsonAsync(
                $"/api/polls/{slug}/votes",
                new VoteRequest { SelectedOptionIds = [optionIds[0]] });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var tally = await response.Content.ReadFromJsonAsync<TallyDto>();
            tally.Should().NotBeNull();
            tally!.Options.Should().NotBeEmpty();
            tally.ShouldHaveCorrectPercentages();
            tally.ShouldSumToApproximately100();
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task SubmitVote_CreatorOnlyPoll_Returns200WithThankYouMessage()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);
            var slug = await CreatePollAndGetSlug(client, visibility: ResultsVisibility.CreatorOnly);
            var optionIds = await GetOptionIds(client, slug);

            var response = await client.PostAsJsonAsync(
                $"/api/polls/{slug}/votes",
                new VoteRequest { SelectedOptionIds = [optionIds[0]] });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            body.Should().NotBeNull();
            body!.Should().ContainKey("message");
            body.Should().NotContainKey("options");
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task SubmitVote_TwoSelectionsForSingleSelect_Returns400()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);
            var slug = await CreatePollAndGetSlug(client);
            var optionIds = await GetOptionIds(client, slug);

            var response = await client.PostAsJsonAsync(
                $"/api/polls/{slug}/votes",
                new VoteRequest { SelectedOptionIds = [optionIds[0], optionIds[1]] });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task SubmitVote_ValidMultiSelect_PersistsAllSelections()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);
            var slug = await CreatePollAndGetSlug(client, mode: PollMode.MultiSelect);
            var optionIds = await GetOptionIds(client, slug);

            var response = await client.PostAsJsonAsync(
                $"/api/polls/{slug}/votes",
                new VoteRequest { SelectedOptionIds = [optionIds[0], optionIds[1]] });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var tally = await response.Content.ReadFromJsonAsync<TallyDto>();
            tally.Should().NotBeNull();
            tally!.Options.Sum(o => o.Votes).Should().Be(2);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task SubmitVote_DuplicateSelections_Returns400()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);
            var slug = await CreatePollAndGetSlug(client, mode: PollMode.MultiSelect);
            var optionIds = await GetOptionIds(client, slug);

            var response = await client.PostAsJsonAsync(
                $"/api/polls/{slug}/votes",
                new VoteRequest { SelectedOptionIds = [optionIds[0], optionIds[0]] });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task GetResults_PublicPoll_Returns200Unauthenticated()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);
            var slug = await CreatePollAndGetSlug(client, visibility: ResultsVisibility.Public);
            var optionIds = await GetOptionIds(client, slug);
            await client.PostAsJsonAsync(
                $"/api/polls/{slug}/votes",
                new VoteRequest { SelectedOptionIds = [optionIds[0]] });

            var unauthClient = factory.CreateClient();
            var response = await unauthClient.GetAsync($"/api/polls/{slug}/results");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var tally = await response.Content.ReadFromJsonAsync<TallyDto>();
            tally.Should().NotBeNull();
            tally!.Options.Should().NotBeEmpty();
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task GetResults_CreatorOnlyUnauthenticated_Returns403()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);
            var slug = await CreatePollAndGetSlug(client, visibility: ResultsVisibility.CreatorOnly);

            var unauthClient = factory.CreateClient();
            var response = await unauthClient.GetAsync($"/api/polls/{slug}/results");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task GetResults_CreatorOnlyByOwner_Returns200()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client);
            var slug = await CreatePollAndGetSlug(client, visibility: ResultsVisibility.CreatorOnly);

            var response = await client.GetAsync($"/api/polls/{slug}/results");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var tally = await response.Content.ReadFromJsonAsync<TallyDto>();
            tally.Should().NotBeNull();
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    [Fact]
    public async Task GetResults_CreatorOnlyByNonOwner_Returns403()
    {
        var (factory, client, dbPath) = CreateFactory();
        try
        {
            await AuthHelper.AuthenticateAsync(client, "creator1");
            var slug = await CreatePollAndGetSlug(client, visibility: ResultsVisibility.CreatorOnly);

            var creator2Client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
            await AuthHelper.AuthenticateAsync(creator2Client, "creator2");

            var response = await creator2Client.GetAsync($"/api/polls/{slug}/results");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally
        {
            factory.Dispose();
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }
}
