using Core.Common.Enums;
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

        public void UpdateSession(
            string sessionId,
            IEnumerable<UAVFieldSubscription> newWantedFields
        )
        {
            Dictionary<int, HashSet<TelemetryFields>>? oldWantedFields =
                _wantedFieldsManager.GetSessionWantedFieldsById(sessionId);

            if (oldWantedFields == null)
            {
                throw new InvalidOperationException($"Session {sessionId} not found");
            }

            _wantedFieldsManager.UpdateWantedUAVFields(sessionId, newWantedFields);
            UpdateUAVConsumers(
                oldWantedFields.Keys,
                newWantedFields.Select(subscription => subscription.TailId)
            );
        }

        public void DeleteSession(string sessionId)
        {
            Dictionary<int, HashSet<TelemetryFields>>? sessionWantedFields =
                _wantedFieldsManager.GetSessionWantedFieldsById(sessionId);
            if (sessionWantedFields == null)
            {
                throw new InvalidOperationException($"Session {sessionId} not found");
            }

            IEnumerable<int> uavIdsToCheck = sessionWantedFields.Keys;
            _wantedFieldsManager.RemoveSession(sessionId);
            CleanupUnusedUAVs(uavIdsToCheck);
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
