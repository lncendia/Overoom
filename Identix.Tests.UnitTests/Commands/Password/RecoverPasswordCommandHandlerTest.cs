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
    // Инициализация mock объекта UserManager.
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

    // Инициализация обработчика.
    _handler = new RecoverPasswordCommandHandler(_userManagerMock.Object);
  }

  /// <summary>
  /// Проверка валидной команды восстановления пароля у пользователя.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_RecoverPassword()
  {
    // Arrange
    // Настройка mock объекта UserManager для возвращения пользователя при вызове FindByIdAsync.
    _userManagerMock
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => new AppUser
      {
        UserName = "test",
        Email = "test@example.com",
        RegistrationTimeUtc = DateTime.UtcNow,
        LastAuthTimeUtc = DateTime.UtcNow
      });

    // Настройка mock объекта UserManager для возвращения успеха при вызове ResetPasswordAsync.
    _userManagerMock
      .Setup(m => m.ResetPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Success);

    // Создаем команду для восстановления пароля у пользователя.
    var command = new RecoverPasswordCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewPassword = "new_password"
    };

    // Act
    // Вызов обработчика команды и ожидание возникновения исключения (если такое есть).
    Exception? exception = await Record.ExceptionAsync(async () =>
    {
      await _handler.Handle(command, CancellationToken.None);
    });

    // Assert
    // Проверка на отсутствие исключения.
    Assert.Null(exception);
  }

  /// <summary>
  /// Проверка случая, когда пользователь не найден по почте.
  /// </summary>
  [Fact]
  public async Task Handle_WhenUserNotFoundByEmail_ThrowsUserNotFoundException()
  {
    // Arrange
    // Настройка mock объекта UserManager для возвращения null при вызове FindByLoginAsync.
    _userManagerMock
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => null);

    // Создаем команду для восстановления пароля у пользователя.
    var command = new RecoverPasswordCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewPassword = "new_password"
    };

    // Act & Assert
    // Проверка, что выполнение метода Handle приводит к возникновению исключения UserNotFoundException.
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда пользователь ввел неверный код подтверждения.
  /// </summary>
  [Fact]
  public async Task Handle_WhenInvalidCode_ThrowsInvalidCodeException()
  {
    // Arrange
    // Настройка mock объекта UserManager для возвращения пользователя при вызове FindByIdAsync.
    _userManagerMock
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => new AppUser
      {
        UserName = "test",
        Email = "test@example.com",
        RegistrationTimeUtc = DateTime.UtcNow,
        LastAuthTimeUtc = DateTime.UtcNow
      });

    // Настройка mock объекта UserManager для возврата ошибки с кодом DuplicateEmail при вызове ResetPasswordAsync.
    _userManagerMock
      .Setup(m => m.ResetPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidToken" }));

    // Создаем команду для восстановления пароля у пользователя.
    var command = new RecoverPasswordCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewPassword = "new_password"
    };

    // Act & Assert
    // Проверка, что выполнение метода Handle приводит к возникновению исключения InvalidCodeException.
    await Assert.ThrowsAsync<InvalidCodeException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда пользователь не найден по почте.
  /// </summary>
  [Fact]
  public async Task Handle_WhenInvalidPassword_ThrowsPasswordValidationException()
  {
    // Arrange
    // Настройка mock объекта UserManager для возвращения пользователя при вызове FindByIdAsync.
    _userManagerMock
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => new AppUser
      {
        UserName = "test",
        Email = "test@example.com",
        RegistrationTimeUtc = DateTime.UtcNow,
        LastAuthTimeUtc = DateTime.UtcNow
      });

    // Настройка mock объекта UserManager для неудачного результата при вызове ResetPasswordAsync.
    _userManagerMock
      .Setup(m => m.ResetPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed());

    // Создаем команду для восстановления пароля у пользователя.
    var command = new RecoverPasswordCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewPassword = "new_password"
    };

    // Act & Assert
    // Проверка, что выполнение метода Handle приводит к возникновению исключения PasswordValidationException.
    await Assert.ThrowsAsync<PasswordValidationException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
