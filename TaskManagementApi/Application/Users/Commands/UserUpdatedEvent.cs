using MediatR;

namespace Contracts;

public class UserUpdatedEvent : INotification
{
    public Guid Id { get; }

    public UserUpdatedEvent(Guid id)
    {
        Id = id;
    }
}
