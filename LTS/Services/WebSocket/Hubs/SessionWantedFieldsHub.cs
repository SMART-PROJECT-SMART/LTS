using Core.Common.Enums;
using LTS.Common;
using Microsoft.AspNetCore.SignalR;
using LTS.Services.SubscriptionManager;
using System.Collections.Concurrent;

namespace LTS.Services.WebSocket.Hubs
{
    public class SessionWantedFieldsHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _sessionsClient = new();

        public SessionWantedFieldsHub()
        {
        }

        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var sessionId = httpContext?.Request.Query[LTSConstants.WebSocket.SESSION_ID_FIELD].ToString();

            if (!string.IsNullOrEmpty(sessionId))
            {
                _sessionsClient[sessionId] = Context.ConnectionId;
                return base.OnConnectedAsync();
            }

            Context.Abort();
            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var sessionToRemove = _sessionsClient.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (sessionToRemove != null)
            {
                _sessionsClient.TryRemove(sessionToRemove, out _);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendTelemetryData(string sessionId,IEnumerable<KeyValuePair<TelemetryFields,double>> telemetryData)
        {
            if (_sessionsClient.TryGetValue(sessionId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync(LTSConstants.WebSocket.RECIVE_TELEMETRY_DATA_METHOD,telemetryData);
            }
        }
    }
}