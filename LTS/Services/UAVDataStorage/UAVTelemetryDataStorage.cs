using System.Collections.Concurrent;
using Core.Common.Enums;

namespace LTS.Services.UAVDataStorage
{
    public class UAVTelemetryDataStorage : IUAVTelemetryDataStorage
    {
        private readonly ConcurrentDictionary<
            int,
            Dictionary<TelemetryFields, double>
        > _uavTelemetryData;

        public UAVTelemetryDataStorage()
        {
            _uavTelemetryData =
                new ConcurrentDictionary<int, Dictionary<TelemetryFields, double>>();
        }

        public void AddNewUAV(int tailId)
        {
            _uavTelemetryData[tailId] = new Dictionary<TelemetryFields, double>();
        }

        public void SaveUAVTelemetryData(
            int tailId,
            IEnumerable<KeyValuePair<TelemetryFields, double>> telemetryData
        )
        {
            _uavTelemetryData[tailId] = telemetryData.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public void DeleteUAV(int tailId)
        {
            _uavTelemetryData.TryRemove(tailId, out _);
        }

        public IEnumerable<KeyValuePair<TelemetryFields, double>> GetUAVTelemetryData(int tailId)
        {
            return _uavTelemetryData[tailId];
        }

        public IEnumerable<(int TailId, IEnumerable<KeyValuePair<TelemetryFields, double>> TelemetryData)> GetAllUAVTelemetryData()
        {
            return _uavTelemetryData.Select(CreateTelemetrySnapshot);
        }

        private static (int TailId, IEnumerable<KeyValuePair<TelemetryFields, double>> TelemetryData) CreateTelemetrySnapshot(
            KeyValuePair<int, Dictionary<TelemetryFields, double>> uavData)
        {
            return (uavData.Key, uavData.Value);
        }
    }
}
