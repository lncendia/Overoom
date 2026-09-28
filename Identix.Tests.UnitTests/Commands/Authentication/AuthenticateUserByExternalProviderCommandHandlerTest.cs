using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.Authentication;

namespace Identix.Tests.UnitTests.Commands.Authentication;

/// <summary>
/// Тестовый класс для AuthenticateUserByExternalProviderCommandHandler
/// </summary>
public class AuthenticateUserByExternalProviderCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly AuthenticateUserByExternalProviderCommandHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public AuthenticateUserByExternalProviderCommandHandlerTest()
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

    _handler = new AuthenticateUserByExternalProviderCommandHandler(_userManagerMock.Object);
  }

  /// <summary>
  /// Проверка валидной команды на аутентификацию.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_Authenticate()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(() => new AppUser
      {
        UserName = "test",
        Email = "test@example.com",
        RegistrationTimeUtc = DateTime.UtcNow,
        LastAuthTimeUtc = DateTime.UtcNow
      });

    _userManagerMock
      .Setup(m => m.IsLockedOutAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(() => false);

    var command = new AuthenticateUserByExternalProviderCommand
    {
      LoginProvider = "TestProvider",
      ProviderKey = "TestKey"
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
  /// Проверка случая, когда пользователь не найден по провайдеру.
  /// </summary>
  [Fact]
  public async Task Handle_WhenUserNotFoundFromProvider_ThrowsUserNotFoundException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(() => null);

    var command = new AuthenticateUserByExternalProviderCommand
    {
      LoginProvider = "TestProvider",
      ProviderKey = "TestKey"
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда пользователь заблокирован.
  /// </summary>
  [Fact]
  public async Task Handle_WhenUserIsLockout_ThrowsUserLockoutException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(() => new AppUser
      {
        UserName = "test",
        Email = "test@example.com",
        RegistrationTimeUtc = DateTime.UtcNow,
        LastAuthTimeUtc = DateTime.UtcNow
      });

    _userManagerMock
      .Setup(m => m.IsLockedOutAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(() => true);

    var command = new AuthenticateUserByExternalProviderCommand
    {
      LoginProvider = "TestProvider",
      ProviderKey = "TestKey"
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserLockoutException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
