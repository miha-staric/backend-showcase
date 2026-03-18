using MediatR;

public record CreateUserCommand(
    String Username,
    String Email,
    Guid? PrimaryAssigneeId,
    DateTime? DueDate
) : IRequest<UserDto>;
