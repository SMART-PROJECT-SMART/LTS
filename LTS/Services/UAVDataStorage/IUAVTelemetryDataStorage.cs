using Core.Common.Enums;

namespace LTS.Services.UAVDataStorage
{
    public interface IUAVTelemetryDataStorage
    {
        public void AddNewUAV(int tailId);
        public void SaveUAVTelemetryData(int tailId,IEnumerable<KeyValuePair<TelemetryFields, double>> telemetryData);
        public void DeleteUAV(int tailId);
        public IEnumerable<KeyValuePair<TelemetryFields, double>> GetUAVTelemetryData(int tailId);
    }
}
