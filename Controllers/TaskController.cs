using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ITenantContext tenantContext, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    // GET: api/tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
    {
        Guid? tenantId = _tenantContext.TenantId;

        if (tenantId == null)
            return NotFound();

        IEnumerable<TaskDto> tasks = await _taskService.GetAllTasksAsync();

        return Ok(tasks);
    }

    // GET: api/tasks/1
    [HttpGet("{taskId}")]
    public async Task<ActionResult<TaskDto>> GetTaskById(Guid taskId)
    {
        Guid? tenantId = _tenantContext.TenantId;

        if (tenantId == null)
            return NotFound();

        TaskDto? task = await _taskService.GetTaskByIdAsync(taskId);

        if (task == null)
            return NotFound();

        return Ok(task);
    }
}
