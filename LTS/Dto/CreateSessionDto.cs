using LTS.Models;

namespace LTS.Dto
{
    public class CreateSessionDto
    {
        public string SessionId { get; set; }
        public IEnumerable<UAVFieldSubscription> WantedFields { get; set; }

        public CreateSessionDto(string sessionId, IEnumerable<UAVFieldSubscription> wantedFields)
        {
            SessionId = sessionId;
            WantedFields = wantedFields;
        }

        public CreateSessionDto() { }
    }
}
