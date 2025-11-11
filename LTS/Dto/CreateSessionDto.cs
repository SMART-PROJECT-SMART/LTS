using Core.Common.Enums;

namespace LTS.Dto
{
    public class CreateSessionDto
    {
        public string SessionId { get; set; }
        public Dictionary<int, IEnumerable<TelemetryFields>> WantedFields { get; set; }

        public CreateSessionDto(
            string sessionId,
            Dictionary<int, IEnumerable<TelemetryFields>> wantedFields
        )
        {
            SessionId = sessionId;
            WantedFields = wantedFields;
        }

        public CreateSessionDto() { }
    }
}
