using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IMediator _mediator;

    public UsersController(
        ILogger<UsersController> logger,
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

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid userId)
    {
        UserDto? user = await _mediator.Send(new GetUserByIdQuery(userId));

        if (user == null)
            return NotFound();

        return Ok(user);
    }
}
