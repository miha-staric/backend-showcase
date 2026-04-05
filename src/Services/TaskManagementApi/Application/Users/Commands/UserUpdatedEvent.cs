using MediatR;

namespace TaskManagementApi.Application.Users.Commands;

public class UserUpdatedEvent(Guid id) : INotification
{
    public Guid Id { get; } = id;
}
