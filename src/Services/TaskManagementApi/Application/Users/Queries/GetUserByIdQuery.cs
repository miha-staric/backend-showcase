using MediatR;
using TaskManagementApi.Dtos.User;

namespace TaskManagementApi.Application.Users.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
