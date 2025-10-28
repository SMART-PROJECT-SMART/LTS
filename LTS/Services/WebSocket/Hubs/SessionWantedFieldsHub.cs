using Core.Common.Enums;
using LTS.Common;
using Microsoft.AspNetCore.SignalR;
using LTS.Services.SubscriptionManager;

namespace LTS.Services.WebSocket.Hubs
{
    public class SessionWantedFieldsHub : Hub
    {
        private readonly IWantedUAVFieldsManager _wantedFieldsManager;

        public SessionWantedFieldsHub(IWantedUAVFieldsManager wantedFieldsManager)
        {
            _wantedFieldsManager = wantedFieldsManager;
        }

        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var sessionId = httpContext?.Request.Query[LTSConstants.WebSocket.SESSION_ID_FIELD].ToString();

            if (!string.IsNullOrEmpty(sessionId))
            {
                if (_wantedFieldsManager.DoesSessionExist(sessionId)) return base.OnConnectedAsync();
            }

            Context.Abort();
            return Task.CompletedTask;

        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}