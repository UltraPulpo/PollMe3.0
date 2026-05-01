using FluentAssertions;
using NSubstitute;
using PollMe.Api.Repositories;
using PollMe.Api.Services;

namespace PollMe.Api.Tests.Services;

public class SlugServiceTests
{
    [Fact]
    public async Task Generate_CollisionOnFirstAttempt_ReturnsSecondCandidate()
    {
        var pollRepo = Substitute.For<IPollRepository>();
        var sut = new SlugService(pollRepo);

        // First call returns true (collision), subsequent calls return false
        pollRepo.SlugExistsAsync(Arg.Any<string>())
                .Returns(true, false);

        var result = await sut.GenerateUniqueSlugAsync();

        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(6);
        await pollRepo.Received(2).SlugExistsAsync(Arg.Any<string>());
    }
}
