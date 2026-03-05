using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    // GET: api/tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks(ITenantService tenantService, IMediator mediator)
    {
        Guid tenantId = tenantService.GetTenantId();
        IEnumerable<TaskDto> tasks = await _taskService.GetAllTasksAsync(tenantId);
        return Ok(tasks);
    }

    // GET: api/tasks/1
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTaskById(ITenantService tenantService, Guid taskId)
    {
        Guid tenantId = tenantService.GetTenantId();

        TaskDto? task = await _taskService.GetTaskByIdAsync(taskId, tenantId);

        if (task == null)
            return NotFound();

        return Ok(task);
    }
}
