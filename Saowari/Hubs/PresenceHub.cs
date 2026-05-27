using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Saowari.Services;

namespace Saowari.Hubs
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                bool wasOffline = !_tracker.IsUserOnline(userId);
                _tracker.UserConnected(userId);

                if (wasOffline)
                {
                    // Notify admins that this user just came online
                    await Clients.Group("Admins").SendAsync("UserIsOnline", userId);
                }
            }

            // If the user is an Admin, add them to the Admins group to receive real-time presence updates
            if (Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("SuperAdmin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                _tracker.UserDisconnected(userId);
                bool isNowOffline = !_tracker.IsUserOnline(userId);

                if (isNowOffline)
                {
                    // Notify admins that this user just went offline
                    await Clients.Group("Admins").SendAsync("UserIsOffline", userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
