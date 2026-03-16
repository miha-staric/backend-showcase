using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateTaskCommandHandler
    : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;

    public CreateTaskCommandHandler(
        AppDbContext db,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
    }

    public async Task<TaskDto> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = _tenantContext.TenantId;

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? throw new Exception("TenantId missing"),
            Title = request.Title,
            AssignedUserId = request.AssignedUserId,
            DueDate = request.DueDate,
            Status = TaskStatus.New
        };

        _db.Tasks.Add(task);

        await _db.SaveChangesAsync(cancellationToken);

        // publish event for other services
        await _publishEndpoint.Publish(new TaskCreatedEvent(task.Id));

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            AssignedUserId = task.AssignedUserId,
            DueDate = task.DueDate,
            Status = task.Status
        };
    }
}
