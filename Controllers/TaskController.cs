using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ILogger<TasksController> _logger;
    private readonly IMediator _mediator;

    public TasksController(
        ILogger<TasksController> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
    {
        IEnumerable<TaskDto?> tasks = await _mediator.Send(new GetTasksQuery());

        return Ok(tasks);
    }

    [HttpGet("{taskId}")]
    public async Task<ActionResult<TaskDto>> GetTaskById(Guid taskId)
    {
        TaskDto? task = await _mediator.Send(new GetTaskByIdQuery(taskId));

        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask(
        [FromBody] CreateTaskCommand command)
    {
        TaskDto? result = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetTaskById),
            new { id = result.Id },
            result);
    }

    [HttpPut("{taskId}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(
      Guid taskId,
      [FromBody] UpdateTaskCommand command)
    {
        if (taskId != command.TaskId)
            return BadRequest("Task ID mismatch");

        TaskDto? updatedTask = await _mediator.Send(command);

        if (updatedTask == null)
            return NotFound();

        return Ok(updatedTask);
    }
}
