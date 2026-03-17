using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IMediator _mediator;

    public UserController(
        ILogger<UserController> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        IEnumerable<UserDto?> tasks = await _mediator.Send(new GetUsersQuery());

        return Ok(tasks);
    }
}
