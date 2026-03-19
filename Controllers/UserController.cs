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
        IEnumerable<UserDto?> users = await _mediator.Send(new GetUsersQuery());

        return Ok(users);
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid userId)
    {
        UserDto? user = await _mediator.Send(new GetUserByIdQuery(userId));

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(
        [FromBody] CreateUserCommand command)
    {
        UserDto? result = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetUserById),
            new { userId = result.Id },
            result);
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<UserDto>> UpdateUser(
      Guid taskId,
      [FromBody] UpdateUserCommand command)
    {
        if (taskId != command.UserId)
            return BadRequest("User ID mismatch");

        UserDto? updatedUser = await _mediator.Send(command);

        if (updatedUser == null)
            return NotFound();

        return Ok(updatedUser);
    }
}
