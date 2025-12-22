using System.Collections.Concurrent;
using Core.Common.Enums;
using LTS.Common;
using LTS.Services.SubscriptionManager;
using Microsoft.AspNetCore.SignalR;

namespace LTS.Services.WebSocket.Hubs
{
    public class SessionWantedFieldsHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _sessionsClient = new();

        public SessionWantedFieldsHub() { }

        public override async Task OnConnectedAsync()
        {
            HttpContext? httpContext = Context.GetHttpContext();
            string? sessionId = httpContext
                ?.Request.Query[LTSConstants.WebSocket.SESSION_ID_FIELD]
                .ToString();

            if (!string.IsNullOrEmpty(sessionId))
            {
                _sessionsClient[sessionId] = Context.ConnectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
                await base.OnConnectedAsync();
                return;
            }

            Context.Abort();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string? sessionToRemove = _sessionsClient
                .FirstOrDefault(x => x.Value == Context.ConnectionId)
                .Key;
            if (sessionToRemove != null)
            {
                _sessionsClient.TryRemove(sessionToRemove, out string _);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionToRemove);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendTelemetryData(
            string sessionId,
            IEnumerable<KeyValuePair<TelemetryFields, double>> telemetryData
        )
        {
            if (_sessionsClient.TryGetValue(sessionId, out string? connectionId))
            {
                await Clients
                    .Client(connectionId)
                    .SendAsync(LTSConstants.WebSocket.RECIVE_TELEMETRY_DATA_METHOD, telemetryData);
            }
        }
    }
}
