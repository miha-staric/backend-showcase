using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using TaskManagementApi.Dtos;
using ZiggyCreatures.Caching.Fusion;
using TaskStatus = Contracts.Enums.TaskStatus;

public class GetTasksQueryHandler(
    AppDbContext dbContext,
    IFusionCache cache,
    ITenantContext tenantContext,
    TaskCacheHelper taskCacheHelper
) : IRequestHandler<GetTasksQuery, IEnumerable<TaskDto?>>
{
    private readonly AppDbContext _db = dbContext;
    private readonly IFusionCache _cache = cache;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly TaskCacheHelper _taskCacheHelper = taskCacheHelper;

    public async Task<IEnumerable<TaskDto?>> Handle(
        GetTasksQuery request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to query tasks.");

        string cacheKey = _taskCacheHelper.GetTasksKey(tenantId);

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                IEnumerable<TaskDto> taskDtos = await _db
                    .Tasks.Include(t => t.PrimaryAssigneeUser)
                    .Select(t => new TaskDto
                    {
                        Id = t.Id,
                        TenantId = t.TenantId,
                        Title = t.Title,
                        Status = t.Status,
                        DueDate = t.DueDate,
                        AssignedUserId = t.PrimaryAssigneeId,
                        AssignedUser =
                            t.PrimaryAssigneeUser == null
                                ? null
                                : new UserDto
                                {
                                    Id = t.PrimaryAssigneeUser.Id,
                                    Username = t.PrimaryAssigneeUser.Username,
                                    Email = t.PrimaryAssigneeUser.Email,
                                },
                    })
                    .ToListAsync();
                return taskDtos;
            }
        );
    }
}
