using System.Collections.Concurrent;
using Core.Common.Enums;
using LTS.Models;
using LTS.Services.UAVTelemetryFieldsReferenceCounter.Interfaces;
using LTS.Services.WantedFieldsManager.Helpers;
using LTS.Services.WantedFieldsManager.Interfaces;

namespace LTS.Services.WantedFieldsManager
{
    public class WantedUavFieldsManager : IWantedUAVFieldsManager
    {
        private readonly ConcurrentDictionary<
            string,
            Dictionary<int, HashSet<TelemetryFields>>
        > _sessionsWantedUAVFields;
        private readonly IUAVTelemetryFieldReferenceCounter _fieldReferenceCounter;

        public WantedUavFieldsManager(IUAVTelemetryFieldReferenceCounter fieldReferenceCounter)
        {
            _sessionsWantedUAVFields =
                new ConcurrentDictionary<string, Dictionary<int, HashSet<TelemetryFields>>>();
            _fieldReferenceCounter = fieldReferenceCounter;
        }

        public void CreateSession(
            string sessionId,
            IEnumerable<UAVFieldSubscription> uavsWantedFields
        )
        {
            Dictionary<int, HashSet<TelemetryFields>> sessionFields =
                SessionFieldsBuilder.BuildFromSubscriptions(uavsWantedFields);

            _sessionsWantedUAVFields[sessionId] = sessionFields;

            foreach (KeyValuePair<int, HashSet<TelemetryFields>> uavFieldsEntry in sessionFields)
            {
                _fieldReferenceCounter.IncrementFieldReferences(uavFieldsEntry.Key, uavFieldsEntry.Value);
            }
        }

        public void UpdateWantedUAVFields(
            string sessionId,
            IEnumerable<UAVFieldSubscription> newWantedUAVsFields
        )
        {
            Dictionary<int, HashSet<TelemetryFields>>? oldSessionFields =
                _sessionsWantedUAVFields.GetValueOrDefault(sessionId);

            if (oldSessionFields == null)
            {
                throw new InvalidOperationException($"Session {sessionId} not found");
            }

            Dictionary<int, HashSet<TelemetryFields>> newSessionFields =
                SessionFieldsBuilder.BuildFromSubscriptions(newWantedUAVsFields);

            _sessionsWantedUAVFields[sessionId] = newSessionFields;

            HashSet<int> allAffectedUavIds = SessionFieldsBuilder.GetAllAffectedUavIds(
                oldSessionFields,
                newSessionFields
            );

            UpdateFieldReferences(oldSessionFields, newSessionFields, allAffectedUavIds);
        }

        public bool RemoveSession(string sessionId)
        {
            if (
                !_sessionsWantedUAVFields.TryRemove(
                    sessionId,
                    out Dictionary<int, HashSet<TelemetryFields>>? removedSession
                )
            )
            {
                return false;
            }

            foreach (KeyValuePair<int, HashSet<TelemetryFields>> uavFieldsEntry in removedSession)
            {
                _fieldReferenceCounter.DecrementFieldReferences(uavFieldsEntry.Key, uavFieldsEntry.Value);
            }

            return true;
        }

        public IEnumerable<int> GetAllWantedUAVs()
        {
            return _fieldReferenceCounter.GetAllWantedUAVs();
        }

        public Dictionary<int, HashSet<TelemetryFields>>? GetSessionWantedFieldsById(
            string sessionId
        )
        {
            return _sessionsWantedUAVFields.GetValueOrDefault(sessionId);
        }

        public IEnumerable<string> GetAllSessionIds()
        {
            return _sessionsWantedUAVFields.Keys;
        }

        public HashSet<TelemetryFields>? GetGlobalWantedFieldsForUAV(int uavId)
        {
            return _fieldReferenceCounter.GetGlobalWantedFieldsForUAV(uavId);
        }

        public bool DoesSessionExist(string sessionId)
        {
            return _sessionsWantedUAVFields.ContainsKey(sessionId);
        }

        private void UpdateFieldReferences(
            Dictionary<int, HashSet<TelemetryFields>> oldSessionFields,
            Dictionary<int, HashSet<TelemetryFields>> newSessionFields,
            HashSet<int> allAffectedUavIds
        )
        {
            foreach (int uavId in allAffectedUavIds)
            {
                HashSet<TelemetryFields>? oldFields = oldSessionFields.GetValueOrDefault(uavId);
                HashSet<TelemetryFields>? newFields = newSessionFields.GetValueOrDefault(uavId);

                if (oldFields != null)
                {
                    _fieldReferenceCounter.DecrementFieldReferences(uavId, oldFields);
                }

                if (newFields != null)
                {
                    _fieldReferenceCounter.IncrementFieldReferences(uavId, newFields);
                }
            }
        }
    }
}
