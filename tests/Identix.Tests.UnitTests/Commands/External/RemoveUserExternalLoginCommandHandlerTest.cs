using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.External;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.External;

namespace Identix.Tests.UnitTests.Commands.External;

/// <summary>
/// Тестовый класс для RemoveUserExternalLoginCommandHandler.
/// </summary>
public class RemoveUserExternalLoginCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly RemoveUserExternalLoginCommandHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public RemoveUserExternalLoginCommandHandlerTest()
  {
    _userManagerMock = new Mock<UserManager<AppUser>>(
      new Mock<IUserStore<AppUser>>().Object,
      new Mock<IOptions<IdentityOptions>>().Object,
      new Mock<IPasswordHasher<AppUser>>().Object,
      Array.Empty<IUserValidator<AppUser>>(),
      Array.Empty<IPasswordValidator<AppUser>>(),
      new Mock<ILookupNormalizer>().Object,
      new Mock<IdentityErrorDescriber>().Object,
      new Mock<IServiceProvider>().Object,
      new Mock<ILogger<UserManager<AppUser>>>().Object);

    _handler = new RemoveUserExternalLoginCommandHandler(_userManagerMock.Object);
  }

  /// <summary>
  /// Проверка валидной команды удаления внешней аутентификации пользователя.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_RemoveExternalLogin()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => new AppUser
      {
        UserName = "test",
        Email = "test@example.com",
        RegistrationTimeUtc = DateTime.UtcNow,
        LastAuthTimeUtc = DateTime.UtcNow
      });

    _userManagerMock
      .Setup(m => m.GetLoginsAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(() => new List<UserLoginInfo>([
        new UserLoginInfo("TestProvider", "TestKey", "TestDisplayName")
      ]));

    var command = new RemoveUserExternalLoginCommand
    {
      UserId = Guid.NewGuid(),
      Provider = "TestProvider"
    };

    // Act
    Exception? exception = await Record.ExceptionAsync(async () =>
    {
      await _handler.Handle(command, CancellationToken.None);
    });

    // Assert
    Assert.Null(exception);
  }

  /// <summary>
  /// Проверка случая, когда пользователь не найден по Id.
  /// </summary>
  [Fact]
  public async Task Handle_WhenUserNotFoundById_ThrowsUserNotFoundException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => null);

    var command = new RemoveUserExternalLoginCommand
    {
      UserId = Guid.NewGuid(),
      Provider = "TestProvider"
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда внешний логин не найден по провайдеру.
  /// </summary>
  [Fact]
  public async Task Handle_WhenLoginFromProviderNotFound_ThrowsLoginNotFoundException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => new AppUser
      {
        UserName = "test",
        Email = "test@example.com",
        RegistrationTimeUtc = DateTime.UtcNow,
        LastAuthTimeUtc = DateTime.UtcNow
      });

    _userManagerMock
      .Setup(m => m.GetLoginsAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(() => new List<UserLoginInfo>());

    var command = new RemoveUserExternalLoginCommand
    {
      UserId = Guid.NewGuid(),
      Provider = "TestProvider"
    };

    // Act & Assert
    await Assert.ThrowsAsync<LoginNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
