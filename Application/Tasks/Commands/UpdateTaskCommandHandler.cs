using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto?>
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _bus;

    public UpdateTaskCommandHandler(AppDbContext db, IPublishEndpoint bus)
    {
        _db = db;
        _bus = bus;
    }

    public async Task<TaskDto?> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId && t.TenantId == request.TenantId);

        if (task == null) return null;

        if (request.Title != null)
            task.Title = request.Title;
        if (request.AssignedUserId.HasValue)
            task.AssignedUserId = request.AssignedUserId;
        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate;
        task.Status = (TaskStatus)request.Status;

        await _db.SaveChangesAsync(cancellationToken);
        await _bus.Publish(new TaskUpdatedEvent(task.Id));

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
