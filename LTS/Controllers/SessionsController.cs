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

        public SessionsController(ISessionManagementService sessionManagementService)
        {
            _sessionManagementService = sessionManagementService;
        }

        [HttpPost]
        public IActionResult CreateSession([FromBody] CreateSessionDto request)
        {
            _sessionManagementService.CreateSession(request.SessionId, request.WantedFields);
            return Ok();
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
    }
}
