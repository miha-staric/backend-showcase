using MediatR;
using TaskManagementApi.Dtos.User;

namespace TaskManagementApi.Application.Users.Queries;

public record GetUsersQuery() : IRequest<IEnumerable<UserDto?>>;
