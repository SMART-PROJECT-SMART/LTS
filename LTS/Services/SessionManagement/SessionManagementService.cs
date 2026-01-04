using Core.Common.Enums;
using LTS.Common;
using LTS.Models;
using LTS.Services.Kafka.UAVTelmetryDataConsumerManager.Interfaces;
using LTS.Services.SessionManagement.Interfaces;
using LTS.Services.UAVDataStorage.Interfaces;
using LTS.Services.WantedFieldsManager.Interfaces;

namespace LTS.Services.SessionManagement
{
    public class SessionManagementService : ISessionManagementService
    {
        private readonly IWantedUAVFieldsManager _wantedFieldsManager;
        private readonly IUAVTelemetryDataKafkaConsumerManager _consumerManager;
        private readonly IUAVTelemetryDataStorage _storage;

        public SessionManagementService(
            IWantedUAVFieldsManager wantedFieldsManager,
            IUAVTelemetryDataKafkaConsumerManager consumerManager,
            IUAVTelemetryDataStorage storage
        )
        {
            _wantedFieldsManager = wantedFieldsManager;
            _consumerManager = consumerManager;
            _storage = storage;
        }

        public void CreateSession(string sessionId, IEnumerable<UAVFieldSubscription> wantedFields)
        {
            _wantedFieldsManager.CreateSession(sessionId, wantedFields);
            RegisterNewUAVs(wantedFields.Select(subscription => subscription.TailId));
        }

        public Result<bool> UpdateSession(
            string sessionId,
            IEnumerable<UAVFieldSubscription> newWantedFields
        )
        {
            if (!_wantedFieldsManager.DoesSessionExist(sessionId))
            {
                return Result<bool>.Fail($"Session {sessionId} not found");
            }

            Dictionary<int, HashSet<TelemetryFields>> oldWantedFields =
                _wantedFieldsManager.GetSessionWantedFieldsById(sessionId)!;

            _wantedFieldsManager.UpdateWantedUAVFields(sessionId, newWantedFields);
            UpdateUAVConsumers(
                oldWantedFields.Keys,
                newWantedFields.Select(subscription => subscription.TailId)
            );

            return Result<bool>.Ok(true);
        }

        public Result<bool> DeleteSession(string sessionId)
        {
            if (!_wantedFieldsManager.DoesSessionExist(sessionId))
            {
                return Result<bool>.Fail($"Session {sessionId} not found");
            }

            Dictionary<int, HashSet<TelemetryFields>> sessionWantedFields =
                _wantedFieldsManager.GetSessionWantedFieldsById(sessionId)!;

            IEnumerable<int> uavIdsToCheck = sessionWantedFields.Keys;
            _wantedFieldsManager.RemoveSession(sessionId);
            CleanupUnusedUAVs(uavIdsToCheck);

            return Result<bool>.Ok(true);
        }

        private void UpdateUAVConsumers(IEnumerable<int> oldUavIds, IEnumerable<int> newUavIds)
        {
            HashSet<int> oldSet = oldUavIds.ToHashSet();
            HashSet<int> newSet = newUavIds.ToHashSet();

            IEnumerable<int> removedUavIds = oldSet.Except(newSet);
            IEnumerable<int> addedUavIds = newSet.Except(oldSet);

            CleanupUnusedUAVs(removedUavIds);
            RegisterNewUAVs(addedUavIds);
        }

        private void RegisterNewUAVs(IEnumerable<int> uavIds)
        {
            foreach (int uavId in uavIds)
            {
                _storage.AddNewUAV(uavId);
                _consumerManager.AddConsumer(uavId.ToString());
            }
        }

        private void CleanupUnusedUAVs(IEnumerable<int> uavIdsToCheck)
        {
            HashSet<int> stillWantedUAVs = _wantedFieldsManager.GetAllWantedUAVs().ToHashSet();

            foreach (int uavId in uavIdsToCheck.Where(id => !stillWantedUAVs.Contains(id)))
            {
                RemoveUAVConsumer(uavId);
            }
        }

        private void RemoveUAVConsumer(int uavId)
        {
            _consumerManager.RemoveConsumer(uavId.ToString());
            _storage.DeleteUAV(uavId);
        }
    }
}
