using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;
using TaskManagementApi.Models;
using UserRole = Contracts.Enums.UserRole;

public class RemoveUserFromTenantCommandHandler(
    AppDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ITenantContext tenantContext,
    UserCacheHelper userCacheHelper
) : IRequestHandler<RemoveUserFromTenantCommand, bool>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly UserCacheHelper _userCacheHelper = userCacheHelper;

    public async Task<bool> Handle(
        RemoveUserFromTenantCommand request,
        CancellationToken cancellationToken
    )
    {
        Guid tenantId =
            _tenantContext.TenantId
            ?? throw new InvalidOperationException("TenantId is required to delete users.");

        if (_tenantContext.UserRole != UserRole.Admin)
            throw new InvalidOperationException(
                "User must have the role of Admin to delete users."
            );

        UserTenant? userTenant = await _dbContext.UserTenant.FirstOrDefaultAsync(
            ut => ut.UserId == request.UserId && ut.TenantId == tenantId,
            cancellationToken
        );

        if (userTenant == null)
            return false;

        _dbContext.UserTenant.Remove(userTenant);

        await _userCacheHelper.InvalidateUserCacheAsync(tenantId, request.UserId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new UserRemovedFromTenantEvent(tenantId, request.UserId));

        return true;
    }
}
