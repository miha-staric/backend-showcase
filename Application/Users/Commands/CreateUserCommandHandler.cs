using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;

public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly UserCacheHelper _userCacheHelper;

    public CreateUserCommandHandler(
        AppDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        UserCacheHelper userCacheHelper)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _userCacheHelper = userCacheHelper;
    }

    public async Task<UserDto> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        Guid tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to create a user.");

        User? user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email
        };

        _dbContext.Users.Add(user);

        UserTenant userTenant = new UserTenant
        {
            UserId = user.Id,
            TenantId = tenantId
        };

        _dbContext.UserTenant.Add(userTenant);

        await _userCacheHelper.InvalidateUserCacheAsync(tenantId, user.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new UserCreatedEvent(user.Id));

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }
}
