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
/// Тестовый класс для ChangePasswordCommandHandler.
/// </summary>
public class ChangePasswordCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly ChangePasswordCommandHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public ChangePasswordCommandHandlerTest()
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

    _handler = new ChangePasswordCommandHandler(_userManagerMock.Object);
  }

  /// <summary>
  /// Проверка валидной команды для изменения пароля у пользователя.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_ChangePassword()
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
      .Setup(m => m.AddPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Success);

    _userManagerMock
      .Setup(m => m.ChangePasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Success);

    var command = new ChangePasswordCommand("old_password", "new_password")
    {
      UserId = Guid.NewGuid()
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
  /// Проверка случая, когда пользователь не найден по id.
  /// </summary>
  [Fact]
  public async Task Handle_WhenUserNotFoundById_ThrowsUserNotFoundException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => null);

    var command = new ChangePasswordCommand("old_password", "new_password")
    {
      UserId = Guid.NewGuid()
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда пользователь не ввел старый пароль.
  /// </summary>
  [Fact]
  public async Task Handle_WhenOldPasswordIsNull_ThrowsOldPasswordNeededException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => new AppUser
      {
        UserName = "test",
        Email = "test@example.com",
        RegistrationTimeUtc = DateTime.UtcNow,
        LastAuthTimeUtc = DateTime.UtcNow,
        PasswordHash = "test_hash"
      });

    var command = new ChangePasswordCommand(null, "new_password")
    {
      UserId = Guid.NewGuid()
    };

    // Act & Assert
    await Assert.ThrowsAsync<PasswordNeededException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда пароль не прошел валидацию.
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
      .Setup(m => m.AddPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed());

    _userManagerMock
      .Setup(m => m.ChangePasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed());

    var command = new ChangePasswordCommand("old_password", "new_password")
    {
      UserId = Guid.NewGuid()
    };

    // Act & Assert
    await Assert.ThrowsAsync<PasswordValidationException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
