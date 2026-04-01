using Contracts;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;

public class RemoveUserFromTenantCommandHandler : IRequestHandler<RemoveUserFromTenantCommand, bool>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly UserCacheHelper _userCacheHelper;

    public RemoveUserFromTenantCommandHandler(
        AppDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        UserCacheHelper userCacheHelper
    )
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _userCacheHelper = userCacheHelper;
    }

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
