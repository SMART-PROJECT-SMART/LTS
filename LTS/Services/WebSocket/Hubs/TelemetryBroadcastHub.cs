using LTS.Common;
using LTS.Models;
using LTS.Services.SessionManagement.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace LTS.Services.WebSocket.Hubs
{
    public class TelemetryBroadcastHub : Hub
    {
        private readonly ISessionManagementService _sessionManagementService;

        public TelemetryBroadcastHub(ISessionManagementService sessionManagementService)
        {
            _sessionManagementService = sessionManagementService;
        }

        public override async Task OnConnectedAsync()
        {
            HttpContext? httpContext = Context.GetHttpContext();
            string? sessionId = httpContext
                ?.Request.Query[LTSConstants.WebSocket.SESSION_ID_FIELD]
                .ToString();

            if (!string.IsNullOrEmpty(sessionId))
            {
                _sessionManagementService.CreateSession(sessionId, Enumerable.Empty<UAVFieldSubscription>());

                Context.Items[LTSConstants.WebSocket.SESSION_ID_KEY] = sessionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

                await Clients.Caller.SendAsync(LTSConstants.WebSocket.SESSION_READY_METHOD, sessionId);

                await base.OnConnectedAsync();
                return;
            }

            Context.Abort();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (
                Context.Items.TryGetValue(
                    LTSConstants.WebSocket.SESSION_ID_KEY,
                    out object? sessionIdObj
                )
            )
            {
                string? sessionId = sessionIdObj as string;
                if (sessionId != null)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
