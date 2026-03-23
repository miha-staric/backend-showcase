using MediatR;

namespace Contracts;

public class UserDeletedEvent : INotification
{
    public Guid UserId { get; }

    public UserDeletedEvent(Guid userId)
    {
        UserId = userId;
    }
}
