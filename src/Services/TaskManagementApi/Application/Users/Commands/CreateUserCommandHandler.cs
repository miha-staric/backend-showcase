using Contracts;
using Contracts.Enums;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.Users.Notifications;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos.User;
using TaskManagementApi.Models;
using TaskManagementApi.Services.Caching;
using TaskManagementApi.Services.Tenancy;
using ZiggyCreatures.Caching.Fusion;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace TaskManagementApi.Application.Users.Commands;

public class CreateUserCommandHandler(
    AppDbContext dbContext,
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    ITenantContext tenantContext,
    IFusionCache cache,
    IValidator<CreateUserCommand> userValidator
) : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IMediator _mediator = mediator;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly IFusionCache _cache = cache;
    private readonly IValidator<CreateUserCommand> _userValidator = userValidator;

    public async Task<UserDto> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken
    )
    {
        ValidationResult validationResult = await _userValidator.ValidateAsync(
            request,
            cancellationToken
        );

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to create users.");

        if (_tenantContext.UserRole != UserRole.Admin)
        {
            throw new InvalidOperationException(
                "User must have the role of Admin to create users."
            );
        }

        User? user = new()
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
        };

        UserTenant? existingUserTenant = await _dbContext.UserTenant.FirstOrDefaultAsync(
            ut => ut.TenantId == tenantId && ut.Username == user.Username,
            cancellationToken: cancellationToken
        );

        if (existingUserTenant != null)
        {
            throw new InvalidOperationException("User with the same username already exists.");
        }

        _ = _dbContext.Users.Add(user);

        UserTenant userTenant = new()
        {
            UserId = user.Id,
            TenantId = tenantId,
            UserRole = request.UserRole,
            Username = user.Username,
        };

        _ = _dbContext.UserTenant.Add(userTenant);

        string cacheKey = UserCacheHelper.GetUsersKey(tenantId);
        await _cache.RemoveAsync(cacheKey, token: cancellationToken);

        _ = await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(
            new UserCreatedEvent(user.Id, user.Email),
            cancellationToken
        );

        await _mediator.Publish(
            new UserCreatedNotification(user.Id, user.Email),
            cancellationToken
        );

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
        };
    }
}
