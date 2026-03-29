using MediatR;

public record GetUsersQuery() : IRequest<IEnumerable<UserDto?>>;
