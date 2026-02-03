using LTS.Dto;
using LTS.Services.Kafka.UAVSnapshotConsumer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LTS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UAVTelemetryDataController : ControllerBase
    {
        private readonly IUAVSnapshotConsumer _snapshotConsumer;
        private readonly ILogger<UAVTelemetryDataController> _logger;

        public UAVTelemetryDataController(
            IUAVSnapshotConsumer snapshotConsumer,
            ILogger<UAVTelemetryDataController> logger
        )
        {
            _snapshotConsumer = snapshotConsumer;
            _logger = logger;
        }

        [HttpGet("all-uav-telemetry-data")]
        public async Task<IActionResult> GetAllUAVTelemetryData()
        {
            IEnumerable<UAVTelemetryDataDto> response = await _snapshotConsumer.PeekAllUAVSnapshots();
            return Ok(response);
        }
    }
}
