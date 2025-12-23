using System.Collections.Concurrent;
using Core.Common.Enums;
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
            IEnumerable<KeyValuePair<int, IEnumerable<TelemetryFields>>> uavsWantedFields
        )
        {
            var sessionFields = new Dictionary<int, HashSet<TelemetryFields>>();

            foreach (
                KeyValuePair<int, IEnumerable<TelemetryFields>> uavWantedFields in uavsWantedFields
            )
            {
                int uavId = uavWantedFields.Key;
                var fieldsSet = new HashSet<TelemetryFields>(uavWantedFields.Value);
                sessionFields[uavId] = fieldsSet;
            }

            _sessionsWantedUAVFields[sessionId] = sessionFields;

            foreach (var uavId in sessionFields.Keys)
            {
                RecalculateGlobalFieldsForUav(uavId);
            }
        }

        public void UpdateWantedUAVFields(
            string sessionId,
            IEnumerable<KeyValuePair<int, IEnumerable<TelemetryFields>>> newWantedUAVsFields
        )
        {
            Dictionary<int, HashSet<TelemetryFields>> existingSession = _sessionsWantedUAVFields[
                sessionId
            ];

            var oldUavIds = new HashSet<int>(existingSession.Keys);
            var newUavIds = new HashSet<int>(newWantedUAVsFields.ToDictionary().Keys);

            var affectedUavIds = new HashSet<int>(oldUavIds);
            affectedUavIds.UnionWith(newUavIds);

            existingSession.Clear();

            foreach (var (uavId, wantedFields) in newWantedUAVsFields)
            {
                existingSession[uavId] = new HashSet<TelemetryFields>(wantedFields);
            }

            foreach (var uavId in affectedUavIds)
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
