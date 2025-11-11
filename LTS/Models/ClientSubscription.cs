using Core.Common.Enums;

namespace LTS.Models
{
    public class ClientSubscription
    {
        public string? ConnectionId { get; set; }
        public string SessionId { get; set; }
        public Dictionary<int, HashSet<TelemetryFields>> WantedUAVsFields;

        public ClientSubscription(
            Dictionary<int, HashSet<TelemetryFields>> wantedUaVsFields,
            string? connectionId,
            string sessionId
        )
        {
            WantedUAVsFields = wantedUaVsFields;
            ConnectionId = connectionId;
            SessionId = sessionId;
        }

        public ClientSubscription(string? connectionId, string sessionId)
        {
            WantedUAVsFields = new Dictionary<int, HashSet<TelemetryFields>>();
            ConnectionId = connectionId;
            SessionId = sessionId;
        }
    }
}
