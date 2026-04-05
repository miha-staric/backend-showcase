using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos.User;
using TaskManagementApi.Models;
using TaskManagementApi.Services.Caching;
using TaskManagementApi.Services.Tenancy;
using UserRole = Contracts.Enums.UserRole;

namespace TaskManagementApi.Application.Users.Commands;

public class UpdateUserCommandHandler(
    AppDbContext db,
    IPublishEndpoint publishEndpoint,
    ITenantContext tenantContext,
    UserCacheHelper userCacheHelper
) : IRequestHandler<UpdateUserCommand, UserDto?>
{
    private readonly AppDbContext _dbContext = db;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly UserCacheHelper _userCacheHelper = userCacheHelper;

    public async Task<UserDto?> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to update users.");

        if (_tenantContext.UserRole != UserRole.Admin)
        {
            throw new InvalidOperationException(
                "User must have the role of Admin to update users."
            );
        }

        User? user = await _dbContext.Users.FirstOrDefaultAsync(
            u => u.Id == request.Id,
            cancellationToken
        );

        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.Username = request.Username ?? user.Username;
        user.Email = request.Email ?? user.Email;

        await _userCacheHelper.InvalidateUserCacheAsync(tenantId, user.Id);

        _ = await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new UserUpdatedEvent(user.Id), cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
        };
    }
}
