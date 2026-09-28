using System.Security.Claims;
using Common.Application.FileStorage;
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
using MassTransit.MongoDbIntegration;

namespace Identix.Tests.UnitTests.Commands.Create;

/// <summary>
/// Тестовый класс для CreateUserExternalCommandHandler.
/// </summary>
public class CreateUserExternalCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле Mock объекта, реализующего IThumbnailStore.
  /// </summary>
  private readonly Mock<IFileStorage> _thumbnailStore = new();

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly CreateUserExternalCommandHandler _handler;

  /// <summary>
  /// Поле, представляющее текущего пользователя с его утверждениями (claims).
  /// </summary>
  private readonly ClaimsPrincipal _claimsPrincipal;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public CreateUserExternalCommandHandlerTest()
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
    var mongoDbContextMock = new Mock<MongoDbContext>();

    _handler = new CreateUserExternalCommandHandler(_userManagerMock.Object, _thumbnailStore.Object,
      publishEndpointMock.Object, mongoDbContextMock.Object,
      new Mock<ILogger<CreateUserExternalCommandHandler>>().Object);

    _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([
      new Claim(ClaimTypes.Email, "test@example.com")
    ]));
  }

  /// <summary>
  /// Проверка валидной команды на добавление внешней аутентификации.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_AddExternalLogin()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(() => null);

    _userManagerMock
      .Setup(m => m.CreateAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(IdentityResult.Success);

    var command = new CreateUserExternalCommand
    {
      LoginInfo = new ExternalLoginInfo(_claimsPrincipal, "TestProvider", "TestKey", "TestDisplayName"),
      Locale = Localization.En
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
  /// Проверка случая, когда логин уже ассоциирован с пользователем.
  /// </summary>
  [Fact]
  public async Task Handle_WhenLoginAlreadyAssociated_ThrowsLoginAlreadyAssociatedException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(() =>
        new AppUser
        {
          UserName = "test",
          Email = "test@example.com",
          RegistrationTimeUtc = DateTime.UtcNow,
          LastAuthTimeUtc = DateTime.UtcNow
        });

    var command = new CreateUserExternalCommand
    {
      LoginInfo = new ExternalLoginInfo(_claimsPrincipal, "TestProvider", "TestKey", "TestDisplayName"),
      Locale = Localization.En
    };

    // Act & Assert
    await Assert.ThrowsAsync<LoginAlreadyAssociatedException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда email не существует.
  /// </summary>
  [Fact]
  public async Task Handle_WhenEmailNotExisted_ThrowsEmailFormatException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(() => null);

    _userManagerMock
      .Setup(m => m.CreateAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(IdentityResult.Success);

    var command = new CreateUserExternalCommand
    {
      LoginInfo = new ExternalLoginInfo(new ClaimsPrincipal(), "TestProvider", "TestKey", "TestDisplayName"),
      Locale = Localization.En
    };

    // Act & Assert
    await Assert.ThrowsAsync<EmailFormatException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда email уже занят.
  /// </summary>
  [Fact]
  public async Task Handle_WhenEmailAlreadyTaken_ThrowsEmailAlreadyTakenException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(() => null);

    _userManagerMock
      .Setup(m => m.CreateAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail" }));

    var command = new CreateUserExternalCommand
    {
      LoginInfo = new ExternalLoginInfo(_claimsPrincipal, "TestProvider", "TestKey", "TestDisplayName"),
      Locale = Localization.En
    };

    // Act & Assert
    await Assert.ThrowsAsync<EmailAlreadyTakenException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда email невалиден.
  /// </summary>
  [Fact]
  public async Task Handle_WhenEmailIsInvalid_ThrowsEmailFormatException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(() => null);

    _userManagerMock
      .Setup(m => m.CreateAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidEmail" }));

    var command = new CreateUserExternalCommand
    {
      LoginInfo = new ExternalLoginInfo(_claimsPrincipal, "TestProvider", "TestKey", "TestDisplayName"),
      Locale = Localization.En
    };

    // Act & Assert
    await Assert.ThrowsAsync<EmailFormatException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка случая, когда длина имени пользователя не валидна.
  /// </summary>
  [Fact]
  public async Task Handle_WhenUsernameLengthIsInvalid_ThrowsUserNameLengthException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()))
      .ReturnsAsync(() => null);

    _userManagerMock
      .Setup(m => m.CreateAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidUserNameLength" }));

    var command = new CreateUserExternalCommand
    {
      LoginInfo = new ExternalLoginInfo(_claimsPrincipal, "TestProvider", "TestKey", "TestDisplayName"),
      Locale = Localization.En
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserNameLengthException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
