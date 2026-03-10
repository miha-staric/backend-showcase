using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ITenantService tenantService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _tenantService = tenantService;
        _logger = logger;
    }

    // GET: api/tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks()
    {
        Guid? tenantId = await _tenantService.GetTenantId();
        if (tenantId == null)
            return NotFound();
        IEnumerable<TaskDto> tasks = await _taskService.GetAllTasksAsync(tenantId);
        return Ok(tasks);
    }

    // GET: api/tasks/1
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTaskById(Guid taskId)
    {
        Guid? tenantId = await _tenantService.GetTenantId();
        if (tenantId == null)
            return NotFound();
        TaskDto? task = await _taskService.GetTaskByIdAsync(taskId, tenantId);

        if (task == null)
            return NotFound();

        return Ok(task);
    }
}
