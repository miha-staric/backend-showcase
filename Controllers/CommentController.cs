using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetAllComments()
    {
        IEnumerable<CommentDto?> comments = await _mediator.Send(new GetCommentsQuery());

        return Ok(comments);
    }

    /*
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid userId)
    {
        UserDto? user = await _mediator.Send(new GetUserByIdQuery(userId));

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserCommand command)
    {
        UserDto? createdUser = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetUserById), new { userId = createdUser.Id }, createdUser);
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<UserDto>> UpdateUser([FromBody] UpdateUserCommand command)
    {
        UserDto? updatedUser = await _mediator.Send(command);

        if (updatedUser == null)
            return NotFound();

        return Ok(updatedUser);
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult> DeleteUser(Guid userId)
    {
        Boolean result = await _mediator.Send(new RemoveUserFromTenantCommand(userId));

        if (!result)
            return NotFound();

        return NoContent();
    }
    */
}
