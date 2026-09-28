using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.Email;

namespace Identix.Tests.UnitTests.Commands.Email;

/// <summary>
/// Тестовый класс для ChangeEmailCommandHandler.
/// </summary>
public class ChangeEmailCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly ChangeEmailCommandHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public ChangeEmailCommandHandlerTest()
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

    _handler = new ChangeEmailCommandHandler(_userManagerMock.Object);
  }

  /// <summary>
  /// Проверка валидной команды смену эл. почты у пользователя.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_ChangeEmail()
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
      .Setup(m => m.ChangeEmailAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Success);

    var command = new ChangeEmailCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewEmail = "test@example.com"
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

    var command = new ChangeEmailCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewEmail = "test@example.com"
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда email уже используется другим пользователем.
  /// </summary>
  [Fact]
  public async Task Handle_WhenEmailAlreadyTaken_ThrowsEmailAlreadyTakenException()
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
      .Setup(m => m.ChangeEmailAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail" }));

    var command = new ChangeEmailCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewEmail = "test@example.com"
    };

    // Act & Assert
    await Assert.ThrowsAsync<EmailAlreadyTakenException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда пользователь ввел неверный код подтверждения.
  /// </summary>
  [Fact]
  public async Task Handle_WhenInvalidToken_ThrowsInvalidCodeException()
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
      .Setup(m => m.ChangeEmailAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidToken" }));

    var command = new ChangeEmailCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewEmail = "test@example.com"
    };

    // Act & Assert
    await Assert.ThrowsAsync<InvalidCodeException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда пользователь ввел неверный код подтверждения.
  /// </summary>
  [Fact]
  public async Task Handle_WhenInvalidEmailFormat_ThrowsEmailFormatException()
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
      .Setup(m => m.ChangeEmailAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidEmail" }));

    var command = new ChangeEmailCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code",
      NewEmail = "test@example.com"
    };

    // Act & Assert
    await Assert.ThrowsAsync<EmailFormatException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
