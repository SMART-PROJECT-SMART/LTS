using Core.Common.Enums;

namespace LTS.Services.SubscriptionManager
{
    public interface IWantedUAVFieldsManager
    {
        void CreateSession(string sessionId, Dictionary<int, IEnumerable<TelemetryFields>> fieldSubscriptions);
        void UpdateWantedUAVFields(string sessionId, Dictionary<int, IEnumerable<TelemetryFields>> fieldSubscriptions);
        bool RemoveSession(string sessionId);
        IEnumerable<int> GetAllWantedUAVs();
        Dictionary<int, HashSet<TelemetryFields>>? GetSessionById(string sessionId);
        IEnumerable<string> GetAllSessionIds();
        HashSet<TelemetryFields>? GetGlobalWantedFieldsForUAV(int uavId);
    }
}