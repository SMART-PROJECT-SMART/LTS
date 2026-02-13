using Core.Common.Enums;
using LTS.Common;
using LTS.Models;
using LTS.Services.ActiveUAVFetcher.Interfaces;
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
        private readonly IUAVFetcher _UAVFetcher;

        public SessionManagementService(
            IWantedUAVFieldsManager wantedFieldsManager,
            IUAVTelemetryDataKafkaConsumerManager consumerManager,
            IUAVTelemetryDataStorage storage,
            IUAVFetcher activeUavFetcher
        )
        {
            _wantedFieldsManager = wantedFieldsManager;
            _consumerManager = consumerManager;
            _storage = storage;
            _UAVFetcher = activeUavFetcher;
        }

        public async void CreateSession(string sessionId, IEnumerable<UAVFieldSubscription> wantedFields, CancellationToken cancellationToken = default)
        {
            try
            {
                IEnumerable<UAVFieldSubscription> expandedFields = await ExpandWildcardSubscriptions(wantedFields, cancellationToken);
                _wantedFieldsManager.CreateSession(sessionId, expandedFields);
                RegisterNewUAVs(expandedFields.Select(subscription => subscription.TailId));
            }
            catch
            {
            }
        }

        public async Task<Result<bool>> UpdateSession(
            string sessionId,
            IEnumerable<UAVFieldSubscription> newWantedFields,
            CancellationToken cancellationToken = default
        )
        {
            if (!_wantedFieldsManager.DoesSessionExist(sessionId))
            {
                return Result<bool>.Fail($"Session {sessionId} not found");
            }

            Dictionary<int, HashSet<TelemetryFields>> oldWantedFields =
                _wantedFieldsManager.GetSessionWantedFieldsById(sessionId)!;

            IEnumerable<UAVFieldSubscription> expandedFields = await ExpandWildcardSubscriptions(newWantedFields, cancellationToken);
            _wantedFieldsManager.UpdateWantedUAVFields(sessionId, expandedFields);
            UpdateUAVConsumers(
                oldWantedFields.Keys,
                expandedFields.Select(subscription => subscription.TailId)
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

        private async Task<IEnumerable<UAVFieldSubscription>> ExpandWildcardSubscriptions(
            IEnumerable<UAVFieldSubscription> subscriptions,
            CancellationToken cancellationToken = default
        )
        {
            UAVFieldSubscription? wildcardSubscription = subscriptions.FirstOrDefault(
                subscription => subscription.TailId == LTSConstants.Subscription.WILDCARD_UAV_ID
            );

            if (wildcardSubscription == null)
            {
                return subscriptions;
            }

            IEnumerable<int> allDiscoveredUavIds = await _UAVFetcher.GetActiveUAVsTailIdAsync(cancellationToken);

            IEnumerable<UAVFieldSubscription> expandedWildcardSubscriptions =
                allDiscoveredUavIds.Select(uavId => new UAVFieldSubscription(
                    uavId,
                    wildcardSubscription.WantedFields
                ));

            IEnumerable<UAVFieldSubscription> nonWildcardSubscriptions = subscriptions.Where(
                subscription => subscription.TailId != LTSConstants.Subscription.WILDCARD_UAV_ID
            );

            return nonWildcardSubscriptions.Concat(expandedWildcardSubscriptions);
        }
    }
}
