using Core.Common.Enums;

namespace LTS.Services.SubscriptionManager
{
    public interface IWantedUAVFieldsManager
    {
        void CreateSession(
            string sessionId,
            IEnumerable<KeyValuePair<int, IEnumerable<TelemetryFields>>> fieldSubscriptions
        );
        void UpdateWantedUAVFields(
            string sessionId,
            IEnumerable<KeyValuePair<int, IEnumerable<TelemetryFields>>> fieldSubscriptions
        );
        bool RemoveSession(string sessionId);
        IEnumerable<int> GetAllWantedUAVs();
        Dictionary<int, HashSet<TelemetryFields>>? GetSessionWantedFieldsById(string sessionId);
        IEnumerable<string> GetAllSessionIds();
        HashSet<TelemetryFields>? GetGlobalWantedFieldsForUAV(int uavId);
        bool DoesSessionExist(string sessionId);
    }
}
