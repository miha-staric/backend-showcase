using MediatR;

public record GetAccessTokenCommand(String username = "carol", String password = "carol123")
    : IRequest<String>;
