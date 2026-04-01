using MediatR;

public record UpdateUserCommand(Guid Id, string? Username = null, string? Email = null)
    : IRequest<UserDto?>;
