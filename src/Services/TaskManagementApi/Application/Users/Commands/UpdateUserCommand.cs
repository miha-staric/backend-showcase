using MediatR;
using TaskManagementApi.Dtos.User;

namespace TaskManagementApi.Application.Users.Commands;

public record UpdateUserCommand(Guid Id, string? Username = null, string? Email = null)
    : IRequest<UserDto?>;
