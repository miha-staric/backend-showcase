using MediatR;
using TaskManagementApi.Dtos;

public record GetUsersQuery() : IRequest<IEnumerable<UserDto?>>;
