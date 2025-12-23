using Core.Common.Enums;

namespace LTS.Dto
{
    public class UAVTelemetryFieldsDto
    {
        public int TailId { get; set; }
        public Dictionary<TelemetryFields, double> Fields { get; set; }

        public UAVTelemetryFieldsDto(int tailId, Dictionary<TelemetryFields, double> fields)
        {
            TailId = tailId;
            Fields = fields;
        }

        public UAVTelemetryFieldsDto() { }
    }
}
