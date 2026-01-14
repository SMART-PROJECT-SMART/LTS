using Core.Common.Enums;
using LTS.Common;
using LTS.Dto;
using LTS.Services.SessionManagement.Interfaces;
using LTS.Services.WantedFieldsManager.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LTS.Controllers
{
    [ApiController]
    [Route("api/wanted-fields")]
    public class WantedFieldsController : ControllerBase
    {
        private readonly ISessionManagementService _sessionManagementService;

        public WantedFieldsController(ISessionManagementService sessionManagementService)
        {
            _sessionManagementService = sessionManagementService;
        }

        [HttpPut("{sessionId}")]
        public IActionResult UpdateSession(
            string sessionId,
            [FromBody] UpdateWantedFieldsDto request
        )
        {
            Result<bool> result = _sessionManagementService.UpdateSession(sessionId, request.WantedFields).Result;

            if (!result.Success)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok();
        }
    }
}
