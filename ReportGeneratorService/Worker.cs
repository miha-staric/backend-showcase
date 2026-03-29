using Contracts;
using MassTransit;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        _logger.LogInformation(
            $"Received: {context.Message}, UserId: {context.Message.UserId}, Email: {context.Message.Email} "
        );

        return Task.CompletedTask;
    }
}
