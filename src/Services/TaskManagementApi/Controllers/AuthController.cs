using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Gets a Keycloak access token for development/testing.
        /// </summary>
        /// <param name="command">The credentials.</param>
        /// <returns>JWT access token.</returns>
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = false)]
        [HttpPost("token")]
        public async Task<IActionResult> GetToken([FromBody] GetAccessTokenCommand command)
        {
            string? result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
