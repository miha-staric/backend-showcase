using MediatR;

public record CreateUserCommand(string Username, string Email, UserRole UserRole)
    : IRequest<UserDto>;
