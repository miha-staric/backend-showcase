using MediatR;

public record GetAccessTokenCommand() : IRequest<string>;
