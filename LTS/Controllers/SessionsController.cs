using Microsoft.AspNetCore.Mvc;
using LTS.Services.SubscriptionManager;
using Core.Common.Enums;
using LTS.Dto;

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
        public IActionResult CreateSession([FromBody] CreateSessionDto request)
        {

            _wantedFieldsManager.CreateSession(request.SessionId, request.WantedFields);

            return Ok(new { SessionId = request.SessionId });
        }

        [HttpPut("{sessionId}")]
        public IActionResult UpdateSession(string sessionId, [FromBody] UpdateWantedFieldsDto request)
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
            Dictionary<int,HashSet<TelemetryFields>>? sessionWantedFields = _wantedFieldsManager.GetSessionWantedFieldsById(sessionId);

            if (sessionWantedFields == null)
            {
                return NotFound($"Session {sessionId} not found");
            }

            return Ok(new { SessionId = sessionId, WantedFields = sessionWantedFields });
        }
    }
}