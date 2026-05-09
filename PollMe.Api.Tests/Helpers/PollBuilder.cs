using PollMe.Api.Dtos;
using PollMe.Api.Models;

namespace PollMe.Api.Tests.Helpers;

public class PollBuilder
{
    private string _question = "Test Question?";
    private string[] _options = ["Option A", "Option B"];
    private PollMode _mode = PollMode.SingleSelect;
    private ResultsVisibility _visibility = ResultsVisibility.Public;

    public PollBuilder WithMode(PollMode m) { _mode = m; return this; }
    public PollBuilder WithVisibility(ResultsVisibility v) { _visibility = v; return this; }
    public PollBuilder WithOptions(params string[] opts) { _options = opts; return this; }

    public CreatePollRequest Build() => new()
    {
        Question = _question,
        Options = _options,
        Mode = _mode,
        Visibility = _visibility
    };
}
