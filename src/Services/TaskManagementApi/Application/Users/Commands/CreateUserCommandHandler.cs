using Contracts;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using ZiggyCreatures.Caching.Fusion;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly AppDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly UserCacheHelper _userCacheHelper;
    private readonly IFusionCache _cache;
    private readonly IValidator<CreateUserCommand> _userValidator;

    public CreateUserCommandHandler(
        AppDbContext dbContext,
        IMediator mediator,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        UserCacheHelper userCacheHelper,
        IFusionCache cache,
        IValidator<CreateUserCommand> userValidator
    )
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _userCacheHelper = userCacheHelper;
        _cache = cache;
        _userValidator = userValidator;
    }

    public async Task<UserDto> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await _userValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to create users.");

        if (_tenantContext.UserRole != UserRole.Admin)
            throw new InvalidOperationException(
                "User must have the role of Admin to create users."
            );

        User? user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
        };

        UserTenant? existingUserTenant = await _dbContext.UserTenant.FirstOrDefaultAsync(ut =>
            ut.TenantId == tenantId && ut.Username == user.Username
        );

        if (existingUserTenant != null)
            throw new Exception("User with the same username already exists.");

        _dbContext.Users.Add(user);

        UserTenant userTenant = new UserTenant
        {
            UserId = user.Id,
            TenantId = tenantId,
            UserRole = request.UserRole,
            Username = user.Username,
        };

        _dbContext.UserTenant.Add(userTenant);

        String cacheKey = _userCacheHelper.GetUsersKey(tenantId);
        await _cache.RemoveAsync(cacheKey);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new UserCreatedEvent(user.Id, user.Email),
            cancellationToken
        );

        await _mediator.Publish(new UserCreatedNotification(user.Id, user.Email));

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
        };
    }
}
