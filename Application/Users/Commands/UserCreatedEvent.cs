using MediatR;

namespace Contracts;

public class UserCreatedEvent : INotification
{
    public Guid UserId { get; }

    public UserCreatedEvent(Guid userId)
    {
        UserId = userId;
    }
}
