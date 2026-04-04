using Contracts.Enums;
using MediatR;
using TaskManagementApi.Dtos;

public record CreateUserCommand(string Username, string Email, UserRole UserRole)
    : IRequest<UserDto>;
