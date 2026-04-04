using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagementApi.Controllers;

namespace tests.Unit.Services
{
    public class UsersControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new UsersController(_mediatorMock.Object);
        }

        [Fact]
        public async Task GetAllUsers_WithValidRequest_ReturnsOkResultWithUsers()
        {
            // Arrange
            List<UserDto> users =
            [
                new UserDto
                {
                    Id = Guid.NewGuid(),
                    Username = "tito",
                    Email = "josip@broz.co.yu",
                    UserRole = UserRole.Admin,
                },
                new UserDto
                {
                    Id = Guid.NewGuid(),
                    Username = "jovanka",
                    Email = "jovanka@broz.co.yu",
                    UserRole = UserRole.User,
                },
            ];
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            // Act
            ActionResult<IEnumerable<UserDto>>? result = await _controller.GetAllUsers();

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            IEnumerable<UserDto> returnedUsers = Assert.IsType<IEnumerable<UserDto>>(
                okResult.Value,
                exactMatch: false
            );
            Assert.Equal(2, returnedUsers.Count());

            _mediatorMock.Verify(
                m => m.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GetUserById_WithValidUserId_ReturnsOkResultWithUser()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            UserDto user = new()
            {
                Id = userId,
                Username = "jovanka",
                Email = "jovanka@broz.co.yu",
                UserRole = UserRole.User,
            };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            ActionResult<UserDto> result = await _controller.GetUserById(userId);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            UserDto returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(userId, returnedUser.Id);
            Assert.Equal("jovanka", returnedUser.Username);
        }

        [Fact]
        public async Task CreateUser_WithValidCommand_ReturnsCreatedAtActionResult()
        {
            // Arrange
            CreateUserCommand command = new(
                Username: "jovanka",
                Email: "jovanka@broz.co.yu",
                UserRole: UserRole.User
            );
            UserDto createdUser = new()
            {
                Id = Guid.NewGuid(),
                Username = command.Username,
                Email = command.Email,
                UserRole = command.UserRole,
            };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdUser);

            // Act
            ActionResult<UserDto> result = await _controller.CreateUser(command);

            // Assert
            CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(
                result.Result
            );
            Assert.Equal(nameof(UsersController.GetUserById), createdResult.ActionName);
            Assert.Equal(createdUser.Id, ((UserDto)createdResult.Value!).Id);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task UpdateUser_WithValidCommand_ReturnsOkResultWithUpdatedUser()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            UpdateUserCommand command = new(
                Id: userId,
                Username: "jane_doe",
                Email: "jane@example.com"
            );
            UserDto updatedUser = new()
            {
                Id = userId,
                Username = command.Username ?? "",
                Email = command.Email ?? "",
            };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedUser);

            // Act
            ActionResult<UserDto> result = await _controller.UpdateUser(command);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            UserDto returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(userId, returnedUser.Id);
            Assert.Equal("jane_doe", returnedUser.Username);
        }
    }
}
