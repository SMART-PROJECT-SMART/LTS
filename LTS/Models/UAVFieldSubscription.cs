using Core.Common.Enums;

namespace LTS.Models
{
    public class UAVFieldSubscription
    {
        public int TailId { get; set; }
        public IEnumerable<TelemetryFields> WantedFields { get; set; }

        public UAVFieldSubscription(int tailId, IEnumerable<TelemetryFields> wantedFields)
        {
            TailId = tailId;
            WantedFields = wantedFields;
        }

        public UAVFieldSubscription() { }
    }
}
