using LTS.Models;

namespace LTS.Dto
{
    public class UpdateWantedFieldsDto
    {
        public IEnumerable<UAVFieldSubscription> WantedFields { get; set; }
    }
}
