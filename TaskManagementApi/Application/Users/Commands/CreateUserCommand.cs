using MediatR;

public record CreateUserCommand(String Username, String Email, UserRole UserRole)
    : IRequest<UserDto>;
