using Core.Common.Enums;
using LTS.Common;
using LTS.Models;

namespace LTS.Services.SessionManagement.Interfaces
{
    public interface ISessionManagementService
    {
        void CreateSession(string sessionId, IEnumerable<UAVFieldSubscription> wantedFields);
        Result<bool> UpdateSession(string sessionId, IEnumerable<UAVFieldSubscription> newWantedFields);
        Result<bool> DeleteSession(string sessionId);
    }
}
