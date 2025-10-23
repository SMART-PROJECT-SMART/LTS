using Core.Common.Enums;

namespace LTS.Models
{
    public struct ClientSubscription
    {
        public string? ConnectionId { get; set; }
        public string SessionId { get; set; }
        public DateTime LastActive { get; set; }
        public Dictionary<int, HashSet<TelemetryFields>> WantedUAVsFields;
    }
}
