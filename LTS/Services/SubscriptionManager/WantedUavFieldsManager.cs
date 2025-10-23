using System.Collections.Concurrent;
using Core.Common.Enums;

namespace LTS.Services.SubscriptionManager
{
    public class WantedUavFieldsManager : IWantedUAVFieldsManager
    {
        private readonly ConcurrentDictionary<string, Dictionary<int, HashSet<TelemetryFields>>>
            _sessionsWantedUAVFields;
        private readonly ConcurrentDictionary<int, HashSet<TelemetryFields>>
            _globalWantedFieldsByUavId;

        public WantedUavFieldsManager()
        {
            _sessionsWantedUAVFields = new ConcurrentDictionary<string, Dictionary<int, HashSet<TelemetryFields>>>();
            _globalWantedFieldsByUavId = new ConcurrentDictionary<int, HashSet<TelemetryFields>>();
        }

        public void CreateSession(string sessionId, Dictionary<int, IEnumerable<TelemetryFields>> uavsWantedFields)
        {
            var sessionFields = new Dictionary<int, HashSet<TelemetryFields>>();

            foreach (var uavWantedFields in uavsWantedFields)
            {
                var uavId = uavWantedFields.Key;
                var fieldsSet = new HashSet<TelemetryFields>(uavWantedFields.Value);
                sessionFields[uavId] = fieldsSet;
            }

            _sessionsWantedUAVFields[sessionId] = sessionFields;

            foreach (var uavId in sessionFields.Keys)
            {
                RecalculateGlobalFieldsForUav(uavId);
            }
        }

        public void UpdateWantedUAVFields(string sessionId, Dictionary<int, IEnumerable<TelemetryFields>> newWantedUAVsFields)
        {
            var affectedUavIds = new HashSet<int>();

            if (_sessionsWantedUAVFields.TryGetValue(sessionId, out var existingSession))
            {
                foreach (var uavId in newWantedUAVsFields.Keys)
                {
                    affectedUavIds.Add(uavId);
                }
            }

            foreach (var newWantedUAVField in newWantedUAVsFields)
            {
                _sessionsWantedUAVFields[sessionId][newWantedUAVField.Key] =
                    new HashSet<TelemetryFields>(newWantedUAVField.Value);
            }

            foreach (var uavId in affectedUavIds)
            {
                RecalculateGlobalFieldsForUav(uavId);
            }
        }

        public bool RemoveSession(string sessionId)
        {
            if (_sessionsWantedUAVFields.TryRemove(sessionId, out var removedSession))
            {
                foreach (var uavId in removedSession.Keys)
                {
                    RecalculateGlobalFieldsForUav(uavId);
                }
                return true;
            }
            return false;
        }

        public IEnumerable<int> GetAllWantedUAVs()
        {
            return _globalWantedFieldsByUavId.Keys;
        }

        public Dictionary<int, HashSet<TelemetryFields>>? GetSessionById(string sessionId)
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
