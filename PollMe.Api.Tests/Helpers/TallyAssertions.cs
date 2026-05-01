using FluentAssertions;
using PollMe.Api.Dtos;

namespace PollMe.Api.Tests.Helpers;

public static class TallyAssertions
{
    public static void ShouldHaveCorrectPercentages(this TallyDto tally)
    {
        var total = tally.Options.Sum(o => o.Votes);
        if (total == 0) return;
        foreach (var opt in tally.Options)
        {
            var expected = (int)Math.Round((double)opt.Votes / total * 100);
            opt.Percentage.Should().Be(expected,
                because: $"option '{opt.Text}' with {opt.Votes} votes out of {total} should have {expected}%");
        }
    }

    public static void ShouldSumToApproximately100(this TallyDto tally)
    {
        var total = tally.Options.Sum(o => o.Votes);
        if (total == 0) return;
        var sum = tally.Options.Sum(o => o.Percentage);
        sum.Should().BeInRange(98, 102,
            because: "percentages should sum to approximately 100");
    }
}
