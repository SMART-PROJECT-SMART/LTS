using Core.Common.Enums;
using LTS.Models;

namespace LTS.Services.SessionManagement.Interfaces
{
    public interface ISessionManagementService
    {
        void CreateSession(string sessionId, IEnumerable<UAVFieldSubscription> wantedFields);
        void UpdateSession(string sessionId, IEnumerable<UAVFieldSubscription> newWantedFields);
        void DeleteSession(string sessionId);
    }
}
