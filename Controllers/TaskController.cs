using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<TasksController> _logger;
    private readonly IMediator _mediator;

    public TasksController(
        ITenantContext tenantContext,
        ILogger<TasksController> logger,
        IMediator mediator
        )
    {
        _tenantContext = tenantContext;
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
    {
        Guid? tenantId = _tenantContext.TenantId;

        if (tenantId == null)
            return NotFound();

        IEnumerable<TaskDto?> tasks = await _mediator.Send(new GetTasksQuery(tenantId.Value));

        return Ok(tasks);
    }

    [HttpGet("{taskId}")]
    public async Task<ActionResult<TaskDto>> GetTaskById(Guid taskId)
    {
        Guid? tenantId = _tenantContext.TenantId;

        if (tenantId == null)
            return NotFound();

        TaskDto? task = await _mediator.Send(new GetTaskByIdQuery(tenantId.Value, taskId));

        if (task == null)
            return NotFound();

        return Ok(task);
    }
}
