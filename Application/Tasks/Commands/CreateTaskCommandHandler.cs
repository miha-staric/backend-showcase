using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateTaskCommandHandler
    : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateTaskCommandHandler(
        AppDbContext db,
        IPublishEndpoint publishEndpoint)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<TaskDto> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
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
