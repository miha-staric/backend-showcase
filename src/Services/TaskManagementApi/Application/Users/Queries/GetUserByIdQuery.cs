using MediatR;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
