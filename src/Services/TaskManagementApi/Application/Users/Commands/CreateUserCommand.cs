using Contracts.Enums;
using MediatR;
using TaskManagementApi.Dtos.User;

namespace TaskManagementApi.Application.Users.Commands;

public record CreateUserCommand(string Username, string Email, UserRole UserRole)
    : IRequest<UserDto>;
