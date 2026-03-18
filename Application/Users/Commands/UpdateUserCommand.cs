using MediatR;

public record UpdateUserCommand(
    Guid UserId,
    String? Username = null,
    String? Email = null
) : IRequest<UserDto?>;
