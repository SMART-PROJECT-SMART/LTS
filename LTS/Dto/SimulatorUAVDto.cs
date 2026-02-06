using Core.Common.Enums;
using Core.Models;

namespace LTS.Dto
{
    public class SimulatorUAVDto
    {
        public int TailId { get; set; }
        public UAVType PlatformType { get; set; }
        public Location BaseLocation { get; set; }
    }
}
