namespace LTS.Dto
{
    public class TelemetryBroadcastDto
    {
        public List<UAVTelemetryFieldsDto> UavData { get; set; }

        public TelemetryBroadcastDto(List<UAVTelemetryFieldsDto> uavData)
        {
            UavData = uavData;
        }

        public TelemetryBroadcastDto() { }
    }
}
