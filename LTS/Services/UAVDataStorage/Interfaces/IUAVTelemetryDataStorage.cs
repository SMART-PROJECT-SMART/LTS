using Core.Common.Enums;
using LTS.Dto;

namespace LTS.Services.UAVDataStorage.Interfaces
{
    public interface IUAVTelemetryDataStorage
    {
        public void AddNewUAV(int tailId);
        public void SaveUAVTelemetryData(
            int tailId,
            IEnumerable<KeyValuePair<TelemetryFields, double>> telemetryData
        );
        public void DeleteUAV(int tailId);
        public IEnumerable<KeyValuePair<TelemetryFields, double>>? GetUAVTelemetryData(int tailId);
        public IEnumerable<UAVTelemetryDataDto> GetAllUAVTelemetryData();
    }
}
