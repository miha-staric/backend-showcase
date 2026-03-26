using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetAllComments()
    {
        IEnumerable<CommentDto?> comments = await _mediator.Send(new GetCommentsQuery());

        return Ok(comments);
    }

    [HttpGet("{commentId}")]
    public async Task<ActionResult<CommentDto>> GetCommentById(Guid commentId)
    {
        CommentDto? comment = await _mediator.Send(new GetCommentByIdQuery(commentId));

        if (comment == null)
            return NotFound();

        return Ok(comment);
    }

    /*

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
