using Microsoft.AspNetCore.SignalR;

namespace Valuator.Hubs;

public class RankHub : Hub
{
    public Task SubscribeToText(string id)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, id);
    }
}