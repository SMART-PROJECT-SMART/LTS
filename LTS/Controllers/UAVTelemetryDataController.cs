using Core.Common.Enums;
using LTS.Dto;
using LTS.Services.UAVDataStorage.Interfaces;
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
            IEnumerable<UAVTelemetryDataDto> response =
                _uavTelemetryDataStorage.GetAllUAVTelemetryData();
            return Ok(response);
        }
    }
}
