using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using TaskManagementApi.Dtos;
using TaskManagementApi.Models;
using UserRole = Contracts.Enums.UserRole;

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
            throw new InvalidOperationException(
                "User must have the role of Admin to update users."
            );

        User? user = await _dbContext.Users.FirstOrDefaultAsync(
            u => u.Id == request.Id,
            cancellationToken
        );

        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.Username = request.Username ?? user.Username;
        user.Email = request.Email ?? user.Email;

        await _userCacheHelper.InvalidateUserCacheAsync(tenantId, user.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new UserUpdatedEvent(user.Id));

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
        };
    }
}
