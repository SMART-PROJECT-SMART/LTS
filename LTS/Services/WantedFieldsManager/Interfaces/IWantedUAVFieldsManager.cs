using Core.Common.Enums;
using LTS.Models;

namespace LTS.Services.WantedFieldsManager.Interfaces
{
    public interface IWantedUAVFieldsManager
    {
        void CreateSession(string sessionId, IEnumerable<UAVFieldSubscription> fieldSubscriptions);
        void UpdateWantedUAVFields(
            string sessionId,
            IEnumerable<UAVFieldSubscription> fieldSubscriptions
        );
        bool RemoveSession(string sessionId);
        IEnumerable<int> GetAllWantedUAVs();
        Dictionary<int, HashSet<TelemetryFields>>? GetSessionWantedFieldsById(string sessionId);
        IEnumerable<string> GetAllSessionIds();
        HashSet<TelemetryFields>? GetGlobalWantedFieldsForUAV(int uavId);
        bool DoesSessionExist(string sessionId);
    }
}
