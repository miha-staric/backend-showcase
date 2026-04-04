using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Dtos;

namespace TaskManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

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

        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateComment(
            [FromBody] CreateCommentCommand command
        )
        {
            CommentDto? createdComment = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetCommentById),
                new { commentId = createdComment.Id },
                createdComment
            );
        }

        [HttpPut]
        public async Task<ActionResult<CommentDto>> UpdateComment(
            [FromBody] UpdateCommentCommand command
        )
        {
            CommentDto? updatedComment = await _mediator.Send(command);

            if (updatedComment == null)
                return NotFound();

            return Ok(updatedComment);
        }

        [HttpDelete("{commentId}")]
        public async Task<ActionResult> DeleteComment(Guid commentId)
        {
            bool result = await _mediator.Send(new DeleteCommentCommand(commentId));

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
