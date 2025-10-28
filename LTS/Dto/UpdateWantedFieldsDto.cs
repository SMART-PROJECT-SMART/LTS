using Core.Common.Enums;

namespace LTS.Dto
{
    public class UpdateWantedFieldsDto
    { 
        public Dictionary<int, IEnumerable<TelemetryFields>> WantedFields { get; set; }

    }
}
