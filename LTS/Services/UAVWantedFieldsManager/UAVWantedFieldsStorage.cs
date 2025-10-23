using System.Collections.Concurrent;
using Core.Common.Enums;

namespace LTS.Services.UAVWantedFieldsManager
{
    public class UAVWantedFieldsManager : IUAVWantedFieldsManager
    {
        private readonly ConcurrentDictionary<int, IEnumerable<TelemetryFields>> _uavWantedFieldsStorage;
        UAVWantedFieldsManager()
        {
            _uavWantedFieldsStorage = new ConcurrentDictionary<int, IEnumerable<TelemetryFields>>();
        }
        public void UpdateUAVWantedTelemetryFields(int tailId, IEnumerable<TelemetryFields> wantedTelemetryFields)
        {
            _uavWantedFieldsStorage[tailId] = wantedTelemetryFields;
        }

        public IEnumerable<TelemetryFields>? GetUAVWantedTelemetryFields(int tailId)
        {
            _uavWantedFieldsStorage.TryGetValue(tailId, out var wantedFields);
            return wantedFields;
        }

        public void RemoveUAV(int tailId)
        {
            _uavWantedFieldsStorage.TryRemove(tailId, out _);
        }
    }
}
