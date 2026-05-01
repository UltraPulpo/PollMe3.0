using Microsoft.AspNetCore.SignalR;

namespace PollMe.Api.Hubs;

public class TallyHub : Hub
{
    public Task JoinPoll(int pollId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"poll-{pollId}");

    public Task LeavePoll(int pollId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"poll-{pollId}");
}
