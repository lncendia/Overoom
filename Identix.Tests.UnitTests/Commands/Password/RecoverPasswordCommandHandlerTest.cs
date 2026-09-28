using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.Password;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.Password;

namespace Identix.Tests.UnitTests.Commands.Password;

/// <summary>
/// Тестовый класс для RecoverPasswordCommandHandler.
/// </summary>
public class RecoverPasswordCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly RecoverPasswordCommandHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public RecoverPasswordCommandHandlerTest()
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

    _handler = new RecoverPasswordCommandHandler(_userManagerMock.Object);
  }

  /// <summary>
  /// Проверка валидной команды восстановления пароля у пользователя.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_RecoverPassword()
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
      .Setup(m => m.ResetPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Success);

    var command = new RecoverPasswordCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewPassword = "new_password"
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
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => null);

    var command = new RecoverPasswordCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewPassword = "new_password"
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда пользователь ввел неверный код подтверждения.
  /// </summary>
  [Fact]
  public async Task Handle_WhenInvalidCode_ThrowsInvalidCodeException()
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
      .Setup(m => m.ResetPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidToken" }));

    var command = new RecoverPasswordCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewPassword = "new_password"
    };

    // Act & Assert
    await Assert.ThrowsAsync<InvalidCodeException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда пользователь не найден по почте.
  /// </summary>
  [Fact]
  public async Task Handle_WhenInvalidPassword_ThrowsPasswordValidationException()
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
      .Setup(m => m.ResetPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed());

    var command = new RecoverPasswordCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewPassword = "new_password"
    };

    // Act & Assert
    await Assert.ThrowsAsync<PasswordValidationException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
