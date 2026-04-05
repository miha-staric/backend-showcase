using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.Tasks.Commands;
using TaskManagementApi.Application.Tasks.Queries;
using TaskManagementApi.Dtos.Task;

namespace TaskManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

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

        return (task == null) ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskCommand command)
    {
        TaskDto? createdTask = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetTaskById), new { taskId = createdTask.Id }, createdTask);
    }

    [HttpPut("{taskId}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(
        Guid taskId,
        [FromBody] UpdateTaskCommand command
    )
    {
        if (taskId != command.Id)
        {
            return BadRequest("Task ID mismatch");
        }

        TaskDto? updatedTask = await _mediator.Send(command);

        return (updatedTask == null) ? NotFound() : Ok(updatedTask);
    }

    [HttpDelete("{taskId}")]
    public async Task<ActionResult> DeleteTask(Guid taskId)
    {
        bool result = await _mediator.Send(new DeleteTaskCommand(taskId));

        return (!result) ? NotFound() : NoContent();
    }
}
