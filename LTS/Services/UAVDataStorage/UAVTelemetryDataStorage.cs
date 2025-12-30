using System.Collections.Concurrent;
using Core.Common.Enums;
using LTS.Dto;
using LTS.Services.UAVDataStorage.Interfaces;

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

        public IEnumerable<KeyValuePair<TelemetryFields, double>>? GetUAVTelemetryData(int tailId)
        {
            return _uavTelemetryData.TryGetValue(
                tailId,
                out Dictionary<TelemetryFields, double>? data
            )
                ? data
                : null;
        }

        public IEnumerable<UAVTelemetryDataDto> GetAllUAVTelemetryData()
        {
            return _uavTelemetryData
                .Select(CreateTelemetrySnapshot)
                .Select(data =>
                {
                    Dictionary<TelemetryFields, double> telemetryDict = data.TelemetryData.ToDictionary();
                    UAVType uavType = ExtractUAVType(telemetryDict);
                    return new UAVTelemetryDataDto(data.TailId, uavType, telemetryDict);
                });
        }

        private (
            int TailId,
            IEnumerable<KeyValuePair<TelemetryFields, double>> TelemetryData
        ) CreateTelemetrySnapshot(KeyValuePair<int, Dictionary<TelemetryFields, double>> uavData)
        {
            return (uavData.Key, uavData.Value);
        }

        private static UAVType ExtractUAVType(Dictionary<TelemetryFields, double> telemetry)
        {
            if (!telemetry.TryGetValue(TelemetryFields.UAVTypeValue, out double typeValue))
            {
                throw new InvalidOperationException(
                    "UAVTypeValue is missing from telemetry data. Ensure the simulation is sending UAVType."
                );
            }

            return (UAVType)(int)typeValue;
        }
    }
}
