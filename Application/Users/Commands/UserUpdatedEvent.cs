using MediatR;

namespace Contracts;

public class UserUpdatedEvent : INotification
{
    public Guid UserId { get; }

    public UserUpdatedEvent(Guid userId)
    {
        UserId = userId;
    }
}
