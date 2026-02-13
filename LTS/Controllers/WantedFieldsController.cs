using LTS.Common;
using LTS.Dto;
using LTS.Services.SessionManagement.Interfaces;
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
        public async Task<IActionResult> UpdateSession(
            string sessionId,
            [FromBody] UpdateWantedFieldsDto request
        )
        {
            Result<bool> result = await _sessionManagementService.UpdateSession(sessionId, request.WantedFields);

            if (!result.Success)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok();
        }
    }
}
