using Core.Common.Enums;
using LTS.Dto;
using LTS.Services.SessionManagement.Interfaces;
using LTS.Services.WantedFieldsManager.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LTS.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionManagementService _sessionManagementService;
        private readonly IWantedUAVFieldsManager _wantedFieldsManager;

        public SessionsController(
            ISessionManagementService sessionManagementService,
            IWantedUAVFieldsManager wantedFieldsManager
        )
        {
            _sessionManagementService = sessionManagementService;
            _wantedFieldsManager = wantedFieldsManager;
        }

        [HttpPost]
        public IActionResult CreateSession([FromBody] CreateSessionDto request)
        {
            _sessionManagementService.CreateSession(request.SessionId, request.WantedFields);
            return Ok(new { SessionId = request.SessionId });
        }

        [HttpPut("{sessionId}")]
        public IActionResult UpdateSession(
            string sessionId,
            [FromBody] UpdateWantedFieldsDto request
        )
        {
            _sessionManagementService.UpdateSession(sessionId, request.WantedFields);
            return Ok();
        }

        [HttpDelete("{sessionId}")]
        public IActionResult DeleteSession(string sessionId)
        {
            _sessionManagementService.DeleteSession(sessionId);
            return NoContent();
        }

        [HttpGet("{sessionId}")]
        public IActionResult GetSession(string sessionId)
        {
            Dictionary<int, HashSet<TelemetryFields>>? sessionWantedFields =
                _wantedFieldsManager.GetSessionWantedFieldsById(sessionId);

            if (sessionWantedFields == null)
            {
                return NotFound($"Session {sessionId} not found");
            }

            return Ok(new { SessionId = sessionId, WantedFields = sessionWantedFields });
        }
    }
}
