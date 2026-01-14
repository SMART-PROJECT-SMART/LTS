using Core.Common.Enums;
using LTS.Common;
using LTS.Models;

namespace LTS.Services.SessionManagement.Interfaces
{
    public interface ISessionManagementService
    {
        void CreateSession(string sessionId, IEnumerable<UAVFieldSubscription> wantedFields, CancellationToken cancellationToken = default);
        Task<Result<bool>> UpdateSession(string sessionId, IEnumerable<UAVFieldSubscription> newWantedFields, CancellationToken cancellationToken = default);
        Result<bool> DeleteSession(string sessionId);
    }
}
