using MediatR;

public record UpdateUserCommand(Guid Id, String? Username = null, String? Email = null)
    : IRequest<UserDto?>;
