using MediatR;

namespace TaskManagementApi.Application.Auth.Commands;

public record GetAccessTokenCommand(string Username = "carol", string Password = "carol123")
    : IRequest<string>;
