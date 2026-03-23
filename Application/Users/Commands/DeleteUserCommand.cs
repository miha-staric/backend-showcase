using MediatR;

public record DeleteUserCommand(Guid UserId) : IRequest<Boolean>;
