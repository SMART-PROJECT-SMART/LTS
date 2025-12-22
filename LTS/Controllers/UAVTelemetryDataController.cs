using Core.Common.Enums;
using LTS.Dto;
using LTS.Services.UAVDataStorage;
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
            var data = CreateMockTelemetryData();
            var response = data.Select(d => new UAVTelemetryDataDto(d.TailId, d.TelemetryData));
            return Ok(response);
        }

        private static IEnumerable<(int TailId, IEnumerable<KeyValuePair<TelemetryFields, double>> TelemetryData)> CreateMockTelemetryData()
        {
            var firstUavTelemetry = new[]
            {
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.TailId,101),
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.Altitude,1200.5),
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.Latitude,32.0853),
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.Longitude,34.7818),
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.CurrentSpeedKmph,250.0),
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.FuelAmount,65.3)
            };

            var secondUavTelemetry = new[]
            {
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.TailId,202),
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.Altitude,800.0),
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.Latitude,29.5581),
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.Longitude,34.9482),
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.CurrentSpeedKmph,180.0),
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.FuelAmount,42.7),
                new KeyValuePair<TelemetryFields, double>(TelemetryFields.DataStorageUsedGB,0)
            };

            return new List<(int, IEnumerable<KeyValuePair<TelemetryFields, double>>)>
            {
                (101, firstUavTelemetry),
                (202, secondUavTelemetry)
            };
        }
    }
}
