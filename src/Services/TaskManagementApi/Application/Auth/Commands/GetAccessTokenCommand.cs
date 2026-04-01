using MediatR;

public record GetAccessTokenCommand(string username = "carol", string password = "carol123")
    : IRequest<string>;
