using Core.Common.Enums;

namespace LTS.Dto
{
    public class UAVTelemetryDataDto
    {
        public int TailId { get; set; }
        public IEnumerable<KeyValuePair<TelemetryFields, double>> TelemetryData { get; set; }

        public UAVTelemetryDataDto(
            int tailId,
            IEnumerable<KeyValuePair<TelemetryFields, double>> telemetryData
        )
        {
            TailId = tailId;
            TelemetryData = telemetryData;
        }
    }
}
