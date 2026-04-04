using MediatR;
using TaskManagementApi.Dtos;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
