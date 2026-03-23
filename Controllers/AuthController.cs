using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    ///
    /// <summary>
    /// Gets a Keycloak access token for development/testing.
    /// </summary>
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = false)]
    [HttpPost("token")]
    public async Task<IActionResult> GetToken()
    {
        String? result = await _mediator.Send(new GetAccessTokenCommand());
        return Ok(result);
    }
}
