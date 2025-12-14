using LTS.Services.UAVDataStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LTS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UAVTelemetryDataController : ControllerBase
    {
        private readonly IUAVTelemetryDataStorage _uavTelemetryDataStorage;
        public UAVTelemetryDataController(IUAVTelemetryDataStorage uavTelemetryDataStorage)
        {
            _uavTelemetryDataStorage = uavTelemetryDataStorage;
        }
        [HttpGet("all-uav-telemetry-data")]
        public IActionResult GetAllUAVTelemetryData()
        {
            return Ok(_uavTelemetryDataStorage.GetAllUAVTelemetryData());
        }
    }
}
