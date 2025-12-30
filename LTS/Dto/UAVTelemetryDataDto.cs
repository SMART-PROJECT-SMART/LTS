using Core.Common.Enums;

namespace LTS.Dto
{
    public class UAVTelemetryDataDto
    {
        public int TailId { get; set; }
        public UAVType UavType { get; set; }
        public Dictionary<TelemetryFields, double> TelemetryData { get; set; }

        public UAVTelemetryDataDto(
            int tailId,
            UAVType uavType,
            Dictionary<TelemetryFields, double> telemetryData
        )
        {
            TailId = tailId;
            UavType = uavType;
            TelemetryData = telemetryData;
        }
    }
}
