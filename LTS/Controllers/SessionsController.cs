using Microsoft.AspNetCore.Mvc;
using LTS.Services.SubscriptionManager;
using Core.Common.Enums;

namespace LTS.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class SessionsController : ControllerBase
    {
        private readonly IWantedUAVFieldsManager _wantedFieldsManager;

        public SessionsController(IWantedUAVFieldsManager wantedFieldsManager)
        {
            _wantedFieldsManager = wantedFieldsManager;
        }

        [HttpPost]
        public IActionResult CreateSession([FromBody] CreateSessionRequest request)
        {

            _wantedFieldsManager.CreateSession(request., request.WantedFields);

            return Ok(new { SessionId = sessionId });
        }

        [HttpPut("{sessionId}")]
        public IActionResult UpdateSession(string sessionId, [FromBody] UpdateSessionRequest request)
        {
            if (_wantedFieldsManager.GetSessionWantedFieldsById(sessionId) != null)
            {
                return NotFound($"Session {sessionId} not found");
            }

            _wantedFieldsManager.UpdateWantedUAVFields(sessionId, request.WantedFields);

            return Ok();
        }

        [HttpDelete("{sessionId}")]
        public IActionResult DeleteSession(string sessionId)
        {
            var removed = _wantedFieldsManager.RemoveSession(sessionId);

            if (!removed)
            {
                return NotFound($"Session {sessionId} not found");
            }

            return NoContent();
        }

        [HttpGet("{sessionId}")]
        public IActionResult GetSession(string sessionId)
        {
            var session = _wantedFieldsManager.GetSessionWantedFieldsById(sessionId);

            if (session == null)
            {
                return NotFound($"Session {sessionId} not found");
            }

            return Ok(new { SessionId = sessionId, WantedFields = session });
        }
    }

    public class CreateSessionRequest
    {
        public Dictionary<int, IEnumerable<TelemetryFields>> WantedFields { get; set; }
    }

    public class UpdateSessionRequest
    {
        public Dictionary<int, IEnumerable<TelemetryFields>> WantedFields { get; set; }
    }
}