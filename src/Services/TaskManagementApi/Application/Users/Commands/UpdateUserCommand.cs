using MediatR;
using TaskManagementApi.Dtos;

public record UpdateUserCommand(Guid Id, string? Username = null, string? Email = null)
    : IRequest<UserDto?>;
