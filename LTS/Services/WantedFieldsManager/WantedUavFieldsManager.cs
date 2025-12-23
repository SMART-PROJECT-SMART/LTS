using System.Collections.Concurrent;
using Core.Common.Enums;
using LTS.Models;
using LTS.Services.WantedFieldsManager.Interfaces;

namespace LTS.Services.SubscriptionManager
{
    public class WantedUavFieldsManager : IWantedUAVFieldsManager
    {
        private readonly ConcurrentDictionary<
            string,
            Dictionary<int, HashSet<TelemetryFields>>
        > _sessionsWantedUAVFields;
        private readonly ConcurrentDictionary<
            int,
            HashSet<TelemetryFields>
        > _globalWantedFieldsByUavId;

        public WantedUavFieldsManager()
        {
            _sessionsWantedUAVFields =
                new ConcurrentDictionary<string, Dictionary<int, HashSet<TelemetryFields>>>();
            _globalWantedFieldsByUavId = new ConcurrentDictionary<int, HashSet<TelemetryFields>>();
        }

        public void CreateSession(
            string sessionId,
            IEnumerable<UAVFieldSubscription> uavsWantedFields
        )
        {
            Dictionary<int, HashSet<TelemetryFields>> sessionFields =
                new Dictionary<int, HashSet<TelemetryFields>>();

            foreach (UAVFieldSubscription subscription in uavsWantedFields)
            {
                HashSet<TelemetryFields> fieldsSet = new HashSet<TelemetryFields>(
                    subscription.WantedFields
                );
                sessionFields[subscription.TailId] = fieldsSet;
            }

            _sessionsWantedUAVFields[sessionId] = sessionFields;

            foreach (int uavId in sessionFields.Keys)
            {
                RecalculateGlobalFieldsForUav(uavId);
            }
        }

        public void UpdateWantedUAVFields(
            string sessionId,
            IEnumerable<UAVFieldSubscription> newWantedUAVsFields
        )
        {
            Dictionary<int, HashSet<TelemetryFields>> existingSession = _sessionsWantedUAVFields[
                sessionId
            ];

            HashSet<int> affectedUavIds = new HashSet<int>(existingSession.Keys);
            existingSession.Clear();

            foreach (UAVFieldSubscription subscription in newWantedUAVsFields)
            {
                existingSession[subscription.TailId] = new HashSet<TelemetryFields>(
                    subscription.WantedFields
                );
                affectedUavIds.Add(subscription.TailId);
            }

            foreach (int uavId in affectedUavIds)
            {
                RecalculateGlobalFieldsForUav(uavId);
            }
        }

        public bool RemoveSession(string sessionId)
        {
            if (!_sessionsWantedUAVFields.TryRemove(sessionId, out var removedSession))
                return false;
            foreach (var uavId in removedSession.Keys)
            {
                RecalculateGlobalFieldsForUav(uavId);
            }
            return true;
        }

        public IEnumerable<int> GetAllWantedUAVs()
        {
            return _globalWantedFieldsByUavId.Keys;
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
            return _globalWantedFieldsByUavId.GetValueOrDefault(uavId);
        }

        public bool DoesSessionExist(string sessionId)
        {
            return _sessionsWantedUAVFields.ContainsKey(sessionId);
        }

        private void RecalculateGlobalFieldsForUav(int uavId)
        {
            var globalFields = new HashSet<TelemetryFields>();

            foreach (var sessionWantedUAVs in _sessionsWantedUAVFields.Values)
            {
                if (sessionWantedUAVs.TryGetValue(uavId, out var wantedFields))
                {
                    globalFields.UnionWith(wantedFields);
                }
            }

            if (globalFields.Count > 0)
            {
                _globalWantedFieldsByUavId[uavId] = globalFields;
            }
            else
            {
                _globalWantedFieldsByUavId.TryRemove(uavId, out _);
            }
        }
    }
}
