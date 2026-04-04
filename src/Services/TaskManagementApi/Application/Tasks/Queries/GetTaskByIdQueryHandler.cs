using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using TaskManagementApi.Dtos;
using ZiggyCreatures.Caching.Fusion;
using TaskStatus = Contracts.Enums.TaskStatus;

public class GetTaskByIdQueryHandler(
    AppDbContext dbContext,
    ITenantContext tenantContext,
    IFusionCache cache,
    TaskCacheHelper taskCacheHelper
) : IRequestHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly AppDbContext _db = dbContext;
    private readonly IFusionCache _cache = cache;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TaskCacheHelper _taskCacheHelper = taskCacheHelper;

    public async Task<TaskDto?> Handle(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to query tasks.");

        string cacheKey = _taskCacheHelper.GetSingleTaskKey(tenantId, request.TaskId);

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                TaskDto? task = await _db
                    .Tasks.Include(t => t.PrimaryAssigneeUser)
                    .Where(t => t.Id == request.TaskId)
                    .Select(t => new TaskDto
                    {
                        Id = t.Id,
                        TenantId = t.TenantId,
                        Title = t.Title,
                        Status = t.Status,
                        DueDate = t.DueDate,
                        AssignedUserId = t.PrimaryAssigneeId,
                        AssignedUser =
                            t.PrimaryAssigneeUser != null
                                ? new UserDto
                                {
                                    Id = t.PrimaryAssigneeUser.Id,
                                    Username = t.PrimaryAssigneeUser.Username,
                                    Email = t.PrimaryAssigneeUser.Email,
                                }
                                : null,
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                return task;
            }
        );
    }
}
