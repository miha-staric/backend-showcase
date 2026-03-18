using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Services.Caching;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto?>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ITenantContext _tenantContext;
    private readonly UserCacheHelper _userCacheHelper;

    public UpdateUserCommandHandler(
        AppDbContext db,
        IPublishEndpoint publishEndpoint,
        ITenantContext tenantContext,
        UserCacheHelper userCacheHelper)
    {
        _dbContext = db;
        _publishEndpoint = publishEndpoint;
        _tenantContext = tenantContext;
        _userCacheHelper = userCacheHelper;
    }

    public async Task<UserDto?> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        Guid tenantId = _tenantContext.TenantId
          ?? throw new InvalidOperationException("TenantId missing");

        User? user = await _dbContext.Users
            .Include(u => u.UserTenants)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        Boolean belongsToTenant = user.UserTenants.Any(ut => ut.TenantId == tenantId);
        if (!belongsToTenant)
            throw new InvalidOperationException("User does not belong to this tenant.");

        user.Username = request.Username ?? user.Username;
        user.Email = request.Email ?? user.Email;

        await _userCacheHelper.InvalidateUserCacheAsync(tenantId, user.Id);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }
}
