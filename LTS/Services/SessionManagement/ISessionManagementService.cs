using Core.Common.Enums;

namespace LTS.Services.SessionManagement
{
    public interface ISessionManagementService
    {
        void CreateSession(
            string sessionId,
            IEnumerable<KeyValuePair<int, IEnumerable<TelemetryFields>>> wantedFields
        );
        void UpdateSession(
            string sessionId,
            IEnumerable<KeyValuePair<int, IEnumerable<TelemetryFields>>> newWantedFields
        );
        void DeleteSession(string sessionId);
    }
}
