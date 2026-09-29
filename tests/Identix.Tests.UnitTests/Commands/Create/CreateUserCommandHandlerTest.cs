using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.Create;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Enums;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.Create;

namespace Identix.Tests.UnitTests.Commands.Create;

/// <summary>
/// Тестовый класс для CreateUserCommandHandler.
/// </summary>
public class CreateUserCommandHandlerTests
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly CreateUserCommandHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public CreateUserCommandHandlerTests()
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

    var publishEndpointMock = new Mock<IPublishEndpoint>();

    _handler = new CreateUserCommandHandler(_userManagerMock.Object, publishEndpointMock.Object);
  }

  /// <summary>
  /// Проверка валидной команды на создание пользователя.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_CreatesUser()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Success);

    var command = new CreateUserCommand
    {
      Email = "test@example.com",
      Password = "P@$$w0rd1",
      Locale = Localization.En,
      ConfirmUrl = "https://example.com/confirm"
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
  /// Проверка случая, когда указан недопустимый email.
  /// </summary>
  [Fact]
  public async Task Handle_WhenEmailIsInvalid_ThrowsEmailFormatException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidEmail" }));

    var command = new CreateUserCommand
    {
      Email = "test@example.com",
      Password = "P@$$w0rd1",
      Locale = Localization.En,
      ConfirmUrl = "https://example.com/confirm"
    };

    // Act & Assert
    await Assert.ThrowsAsync<EmailFormatException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда email уже используется другим пользователем.
  /// </summary>
  [Fact]
  public async Task Handle_WhenEmailAlreadyTaken_ThrowsEmailAlreadyTakenException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail" }));

    var command = new CreateUserCommand
    {
      Email = "test@example.com",
      Password = "P@$$w0rd1",
      Locale = Localization.En,
      ConfirmUrl = "https://example.com/confirm"
    };

    // Act & Assert
    await Assert.ThrowsAsync<EmailAlreadyTakenException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда пароль не проходит валидацию.
  /// </summary>
  [Fact]
  public async Task Handle_WhenInvalidPassword_ThrowsPasswordValidationException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed());

    var command = new CreateUserCommand
    {
      Email = "test@example.com",
      Password = "P@$$w0rd1",
      Locale = Localization.En,
      ConfirmUrl = "https://example.com/confirm"
    };

    // Act & Assert
    await Assert.ThrowsAsync<PasswordValidationException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда длина имени пользователя не валидна.
  /// </summary>
  [Fact]
  public async Task Handle_WhenUsernameLengthIsInvalid_ThrowsUserNameLengthException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidUserNameLength" }));

    var command = new CreateUserCommand
    {
      Email = "test@example.com",
      Password = "P@$$w0rd1",
      Locale = Localization.En,
      ConfirmUrl = "https://example.com/confirm"
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserNameLengthException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
