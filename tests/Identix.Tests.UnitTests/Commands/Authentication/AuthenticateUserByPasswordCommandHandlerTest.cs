using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.Authentication;
using MassTransit;

namespace Identix.Tests.UnitTests.Commands.Authentication;

/// <summary>
/// Тестовый класс для AuthenticateUserByPasswordCommandHandler
/// </summary>
public class AuthenticateUserByPasswordCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly AuthenticateUserByPasswordCommandHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public AuthenticateUserByPasswordCommandHandlerTest()
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

    var busMock = new Mock<IBus>();
    _handler = new AuthenticateUserByPasswordCommandHandler(_userManagerMock.Object, busMock.Object);
  }

  /// <summary>
  /// Проверка валидной команды на аутентификацию по паролю.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_AuthenticateByPassword()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
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

    _userManagerMock
      .Setup(m => m.IsEmailConfirmedAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(() => true);

    _userManagerMock
      .Setup(m => m.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
      .ReturnsAsync(() => true);


    var command = new AuthenticateUserByPasswordCommand
    {
      Email = "test@example.com",
      Password = "password",
      ConfirmUrl = "https://google.com"
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
  /// Проверка случая, когда пользователь не найден по почте.
  /// </summary>
  [Fact]
  public async Task Handle_WhenUserNotFoundByEmail_ThrowsUserNotFoundException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
      .ReturnsAsync(() => null);

    var command = new AuthenticateUserByPasswordCommand
    {
      Email = "test@example.com",
      Password = "password",
      ConfirmUrl = "https://google.com"
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
      .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
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

    var command = new AuthenticateUserByPasswordCommand
    {
      Email = "test@example.com",
      Password = "password",
      ConfirmUrl = "https://google.com"
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserLockoutException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда пользователь ввел неверный пароль.
  /// </summary>
  [Fact]
  public async Task Handle_WhenWrongPassword_ThrowsInvalidPasswordException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
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

    _userManagerMock
      .Setup(m => m.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
      .ReturnsAsync(() => false);

    var command = new AuthenticateUserByPasswordCommand
    {
      Email = "test@example.com",
      Password = "password",
      ConfirmUrl = "https://google.com"
    };

    // Act & Assert
    await Assert.ThrowsAsync<InvalidPasswordException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
