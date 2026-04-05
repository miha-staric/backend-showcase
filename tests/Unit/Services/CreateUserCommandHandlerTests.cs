using Contracts;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagementApi.Application.Users.Commands;
using TaskManagementApi.Application.Users.Notifications;
using TaskManagementApi.Data;
using TaskManagementApi.Dtos.User;
using TaskManagementApi.Models;
using TaskManagementApi.Services.Caching;
using TaskManagementApi.Services.Tenancy;
using ZiggyCreatures.Caching.Fusion;
using UserRole = Contracts.Enums.UserRole;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace tests.Unit.Services;

public class CreateUserCommandHandlerTests
{
    private static AppDbContext CreateDbContext(ITenantContext tenantContext)
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, tenantContext);
    }

    private static Mock<IValidator<CreateUserCommand>> CreateValidValidator()
    {
        Mock<IValidator<CreateUserCommand>> validatorMock = new();

        ValidationResult validResult = new();

        _ = validatorMock
            .Setup(static v =>
                v.ValidateAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(validResult);

        return validatorMock;
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesUser_AndPublishesEvents()
    {
        // Arrange
        Mock<ITenantContext> tenantContextMock = new();

        _ = tenantContextMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());
        _ = tenantContextMock.Setup(x => x.UserRole).Returns(UserRole.Admin);

        AppDbContext dbContext = CreateDbContext(tenantContextMock.Object);

        Mock<IMediator> mediatorMock = new();
        Mock<IPublishEndpoint> publishEndpointMock = new();
        Mock<IFusionCache> cacheMock = new();
        Mock<IValidator<CreateUserCommand>> validatorMock = CreateValidValidator();

        UserCacheHelper cacheHelper = new(cacheMock.Object);

        Guid tenantId = Guid.NewGuid();

        _ = tenantContextMock.Setup(x => x.TenantId).Returns(tenantId);
        _ = tenantContextMock.Setup(x => x.UserRole).Returns(UserRole.Admin);

        CreateUserCommandHandler handler = new(
            dbContext,
            mediatorMock.Object,
            publishEndpointMock.Object,
            tenantContextMock.Object,
            cacheMock.Object,
            validatorMock.Object
        );

        CreateUserCommand command = new(
            Username: "tito",
            Email: "josip@broz.co.yu",
            UserRole: UserRole.User
        );

        // Act
        UserDto result = await handler.Handle(command, CancellationToken.None);

        // Assert - DB
        List<User> users = [.. dbContext.Users];
        List<UserTenant> userTenants = [.. dbContext.UserTenant];

        _ = Assert.Single(users);
        _ = Assert.Single(userTenants);

        // Assert - result
        Assert.Equal("tito", result.Username);
        Assert.Equal("josip@broz.co.yu", result.Email);

        // Assert - cache
        cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);

        // Assert - event
        publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        // Assert - notification
        mediatorMock.Verify(
            m => m.Publish(It.IsAny<UserCreatedNotification>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_InvalidValidation_ThrowsValidationException()
    {
        // Arrange
        Mock<ITenantContext> tenantContextMock = new();
        Guid tenantId = Guid.NewGuid();
        _ = tenantContextMock.Setup(x => x.TenantId).Returns(tenantId);
        _ = tenantContextMock.Setup(x => x.UserRole).Returns(UserRole.Admin);
        AppDbContext dbContext = CreateDbContext(tenantContextMock.Object);

        Mock<IMediator> mediatorMock = new();
        Mock<IPublishEndpoint> publishEndpointMock = new();
        Mock<IFusionCache> cacheMock = new();

        Mock<IValidator<CreateUserCommand>> validatorMock = new();

        List<ValidationFailure> failures = [new ValidationFailure("Username", "Required")];

        ValidationResult invalidResult = new(failures);

        _ = validatorMock
            .Setup(v =>
                v.ValidateAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(invalidResult);

        UserCacheHelper cacheHelper = new(cacheMock.Object);

        CreateUserCommandHandler handler = new(
            dbContext,
            mediatorMock.Object,
            publishEndpointMock.Object,
            tenantContextMock.Object,
            cacheMock.Object,
            validatorMock.Object
        );

        CreateUserCommand command = new("", "", UserRole.Admin);

        // Act & Assert
        _ = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_UserNotAdmin_ThrowsInvalidOperationException()
    {
        // Arrange
        Mock<ITenantContext> tenantContextMock = new();
        Guid tenantId = Guid.NewGuid();
        _ = tenantContextMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());
        _ = tenantContextMock.Setup(x => x.UserRole).Returns(UserRole.User);
        AppDbContext dbContext = CreateDbContext(tenantContextMock.Object);
        Mock<IMediator> mediatorMock = new();
        Mock<IPublishEndpoint> publishEndpointMock = new();
        Mock<IFusionCache> cacheMock = new();
        Mock<IValidator<CreateUserCommand>> validatorMock = CreateValidValidator();

        UserCacheHelper cacheHelper = new(cacheMock.Object);

        CreateUserCommandHandler handler = new(
            dbContext,
            mediatorMock.Object,
            publishEndpointMock.Object,
            tenantContextMock.Object,
            cacheMock.Object,
            validatorMock.Object
        );

        CreateUserCommand command = new("", "", UserRole.User);

        // Act & Assert
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_DuplicateUser_ThrowsException()
    {
        // Arrange
        Mock<ITenantContext> tenantContextMock = new();
        Guid tenantId = Guid.NewGuid();
        _ = tenantContextMock.Setup(x => x.TenantId).Returns(tenantId);
        _ = tenantContextMock.Setup(x => x.UserRole).Returns(UserRole.Admin);
        AppDbContext dbContext = CreateDbContext(tenantContextMock.Object);

        // Create existing user with a tenant
        User existingUser = new()
        {
            Id = Guid.NewGuid(),
            Username = "tito",
            Email = "josip@broz.co.yu",
            UserTenants =
            [
                new()
                {
                    TenantId = tenantId,
                    UserRole = UserRole.User,
                    Username = "tito",
                },
            ],
        };

        _ = dbContext.Users.Add(existingUser);
        _ = await dbContext.SaveChangesAsync();

        Mock<IMediator> mediatorMock = new();
        Mock<IPublishEndpoint> publishEndpointMock = new();
        Mock<IFusionCache> cacheMock = new();
        Mock<IValidator<CreateUserCommand>> validatorMock = CreateValidValidator();
        UserCacheHelper cacheHelper = new(cacheMock.Object);

        CreateUserCommandHandler handler = new(
            dbContext,
            mediatorMock.Object,
            publishEndpointMock.Object,
            tenantContextMock.Object,
            cacheMock.Object,
            validatorMock.Object
        );

        CreateUserCommand command = new(
            Username: "tito",
            Email: "josip@broz.co.yu",
            UserRole: UserRole.User
        );

        // Act & Assert
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None)
        );
    }
}
