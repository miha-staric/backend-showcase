using Contracts;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Notifications;
using Services.Caching;
using ZiggyCreatures.Caching.Fusion;
using ValidationResult = FluentValidation.Results.ValidationResult;

public class CreateUserCommandHandlerTests
{
    private AppDbContext CreateDbContext(ITenantContext tenantContext)
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, tenantContext);
    }

    private Mock<IValidator<CreateUserCommand>> CreateValidValidator()
    {
        Mock<IValidator<CreateUserCommand>> validatorMock =
            new Mock<IValidator<CreateUserCommand>>();

        ValidationResult validResult = new ValidationResult();

        validatorMock
            .Setup(v =>
                v.ValidateAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(validResult);

        return validatorMock;
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesUser_AndPublishesEvents()
    {
        // Arrange
        Mock<ITenantContext> tenantContextMock = new Mock<ITenantContext>();

        tenantContextMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());
        tenantContextMock.Setup(x => x.UserRole).Returns(UserRole.Admin);

        AppDbContext dbContext = CreateDbContext(tenantContextMock.Object);

        Mock<IMediator> mediatorMock = new Mock<IMediator>();
        Mock<IPublishEndpoint> publishEndpointMock = new Mock<IPublishEndpoint>();
        Mock<IFusionCache> cacheMock = new Mock<IFusionCache>();
        Mock<IValidator<CreateUserCommand>> validatorMock = CreateValidValidator();

        UserCacheHelper cacheHelper = new UserCacheHelper(cacheMock.Object);

        Guid tenantId = Guid.NewGuid();

        tenantContextMock.Setup(x => x.TenantId).Returns(tenantId);
        tenantContextMock.Setup(x => x.UserRole).Returns(UserRole.Admin);

        CreateUserCommandHandler handler = new CreateUserCommandHandler(
            dbContext,
            mediatorMock.Object,
            publishEndpointMock.Object,
            tenantContextMock.Object,
            cacheHelper,
            cacheMock.Object,
            validatorMock.Object
        );

        CreateUserCommand command = new CreateUserCommand(
            Username: "tito",
            Email: "josip@broz.co.yu",
            UserRole: UserRole.User
        );

        // Act
        UserDto result = await handler.Handle(command, CancellationToken.None);

        // Assert - DB
        List<User> users = dbContext.Users.ToList();
        List<UserTenant> userTenants = dbContext.UserTenant.ToList();

        Assert.Single(users);
        Assert.Single(userTenants);

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
        Mock<ITenantContext> tenantContextMock = new Mock<ITenantContext>();
        Guid tenantId = Guid.NewGuid();
        tenantContextMock.Setup(x => x.TenantId).Returns(tenantId);
        tenantContextMock.Setup(x => x.UserRole).Returns(UserRole.Admin);
        AppDbContext dbContext = CreateDbContext(tenantContextMock.Object);

        Mock<IMediator> mediatorMock = new Mock<IMediator>();
        Mock<IPublishEndpoint> publishEndpointMock = new Mock<IPublishEndpoint>();
        Mock<IFusionCache> cacheMock = new Mock<IFusionCache>();

        Mock<IValidator<CreateUserCommand>> validatorMock =
            new Mock<IValidator<CreateUserCommand>>();

        List<ValidationFailure> failures = new List<ValidationFailure>
        {
            new ValidationFailure("Username", "Required"),
        };

        ValidationResult invalidResult = new ValidationResult(failures);

        validatorMock
            .Setup(v =>
                v.ValidateAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(invalidResult);

        UserCacheHelper cacheHelper = new UserCacheHelper(cacheMock.Object);

        CreateUserCommandHandler handler = new CreateUserCommandHandler(
            dbContext,
            mediatorMock.Object,
            publishEndpointMock.Object,
            tenantContextMock.Object,
            cacheHelper,
            cacheMock.Object,
            validatorMock.Object
        );

        CreateUserCommand command = new CreateUserCommand("", "", UserRole.Admin);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_UserNotAdmin_ThrowsInvalidOperationException()
    {
        // Arrange
        Mock<ITenantContext> tenantContextMock = new Mock<ITenantContext>();
        Guid tenantId = Guid.NewGuid();
        tenantContextMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());
        tenantContextMock.Setup(x => x.UserRole).Returns(UserRole.User);
        AppDbContext dbContext = CreateDbContext(tenantContextMock.Object);
        Mock<IMediator> mediatorMock = new Mock<IMediator>();
        Mock<IPublishEndpoint> publishEndpointMock = new Mock<IPublishEndpoint>();
        Mock<IFusionCache> cacheMock = new Mock<IFusionCache>();
        Mock<IValidator<CreateUserCommand>> validatorMock = CreateValidValidator();

        UserCacheHelper cacheHelper = new UserCacheHelper(cacheMock.Object);

        CreateUserCommandHandler handler = new CreateUserCommandHandler(
            dbContext,
            mediatorMock.Object,
            publishEndpointMock.Object,
            tenantContextMock.Object,
            cacheHelper,
            cacheMock.Object,
            validatorMock.Object
        );

        CreateUserCommand command = new CreateUserCommand("", "", UserRole.User);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_DuplicateUser_ThrowsException()
    {
        // Arrange
        Mock<ITenantContext> tenantContextMock = new Mock<ITenantContext>();
        Guid tenantId = Guid.NewGuid();
        tenantContextMock.Setup(x => x.TenantId).Returns(tenantId);
        tenantContextMock.Setup(x => x.UserRole).Returns(UserRole.Admin);
        AppDbContext dbContext = CreateDbContext(tenantContextMock.Object);

        // Create existing user with a tenant
        User existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "tito",
            Email = "josip@broz.co.yu",
            UserTenants = new List<UserTenant>
            {
                new UserTenant
                {
                    TenantId = tenantId,
                    UserRole = UserRole.User,
                    Username = "tito",
                },
            },
        };

        dbContext.Users.Add(existingUser);
        await dbContext.SaveChangesAsync();

        Mock<IMediator> mediatorMock = new Mock<IMediator>();
        Mock<IPublishEndpoint> publishEndpointMock = new Mock<IPublishEndpoint>();
        Mock<IFusionCache> cacheMock = new Mock<IFusionCache>();
        Mock<IValidator<CreateUserCommand>> validatorMock = CreateValidValidator();
        UserCacheHelper cacheHelper = new UserCacheHelper(cacheMock.Object);

        CreateUserCommandHandler handler = new CreateUserCommandHandler(
            dbContext,
            mediatorMock.Object,
            publishEndpointMock.Object,
            tenantContextMock.Object,
            cacheHelper,
            cacheMock.Object,
            validatorMock.Object
        );

        CreateUserCommand command = new CreateUserCommand(
            Username: "tito",
            Email: "josip@broz.co.yu",
            UserRole: UserRole.User
        );

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }
}
