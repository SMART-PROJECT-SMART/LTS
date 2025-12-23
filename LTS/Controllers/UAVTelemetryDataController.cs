using LTS.Dto;
using LTS.Services.Kafka.UAVSnapshotConsumer.Interfaces;
using LTS.Services.UAVTopicDiscovery.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LTS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UAVTelemetryDataController : ControllerBase
    {
        private readonly IUAVTopicDiscoveryService _topicDiscoveryService;
        private readonly IUAVSnapshotConsumer _snapshotConsumer;
        private readonly ILogger<UAVTelemetryDataController> _logger;

        public UAVTelemetryDataController(
            IUAVTopicDiscoveryService topicDiscoveryService,
            IUAVSnapshotConsumer snapshotConsumer,
            ILogger<UAVTelemetryDataController> logger
        )
        {
            _topicDiscoveryService = topicDiscoveryService;
            _snapshotConsumer = snapshotConsumer;
            _logger = logger;
        }

        [HttpGet("all-uav-telemetry-data")]
        public IActionResult GetAllUAVTelemetryData()
        {
            IEnumerable<UAVTelemetryDataDto> response = _snapshotConsumer.PeekAllUAVSnapshots();
            return Ok(response);
        }

        [HttpPost("refresh-uav-topics")]
        public async Task<IActionResult> RefreshUAVTopics()
        {
            await _topicDiscoveryService.DiscoverAndCacheUAVTopicsAsync();
            return Ok(new { Message = "UAV topics refreshed successfully" });
        }

        [HttpPost("add-uav-topic/{tailId}")]
        public IActionResult AddUAVTopic(int tailId)
        {
            _topicDiscoveryService.AddUAVTopic(tailId);
            return Ok(new { Message = $"UAV topic {tailId} added successfully" });
        }
    }
}
