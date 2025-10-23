using System.Collections.Concurrent;
using Core.Common.Enums;

namespace LTS.Services.SubscriptionManager
{
    public class WantedUavFieldsManager : IWantedUAVFieldsManager
    {
        private readonly ConcurrentDictionary<string, Dictionary<int, HashSet<TelemetryFields>>>
            _sessionsWantedUAVFields;
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
        }

        public void UpdateWantedUAVFields(string sessionId, Dictionary<int, IEnumerable<TelemetryFields>> newWantedUAVsFields)
        {
            foreach (var newWantedUAVField in newWantedUAVsFields)
            {
                _sessionsWantedUAVFields[sessionId][newWantedUAVField.Key] =
                    new HashSet<TelemetryFields>(newWantedUAVField.Value);
            }
        }

        public bool RemoveSession(string sessionId)
        {
            return _sessionsWantedUAVFields.TryRemove(sessionId, out _);
        }

        public IEnumerable<int> GetAllWantedUAVs()
        {
            var allWantedUAVs = new HashSet<int>();
            foreach (var sessionWantedUAVs in _sessionsWantedUAVFields.Values)
            {
                foreach (var uavId in sessionWantedUAVs.Keys)
                {
                    allWantedUAVs.Add(uavId);
                }
            }
            return allWantedUAVs;
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
            var globalWantedUAVFields =  new HashSet<TelemetryFields>();
            foreach (var sessionWantedUAVs in _sessionsWantedUAVFields.Values)
            {
                if (sessionWantedUAVs.TryGetValue(uavId, out var wantedFields))
                {
                    globalWantedUAVFields.UnionWith(wantedFields);
                }
            }
            return globalWantedUAVFields.Count > 0 ? globalWantedUAVFields : null;
        }
    }
}
