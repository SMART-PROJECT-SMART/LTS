using Core.Common.Enums;

namespace LTS.Services.UAVWantedFieldsManager
{
    public interface IUAVWantedFieldsManager
    {
        public void UpdateUAVWantedTelemetryFields(int tailId, IEnumerable<TelemetryFields> wantedTelemetryFields);
        public IEnumerable<TelemetryFields>? GetUAVWantedTelemetryFields(int tailId);

        public void RemoveUAV(int tailId);
    }
}
