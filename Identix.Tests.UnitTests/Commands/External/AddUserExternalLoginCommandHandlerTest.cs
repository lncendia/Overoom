using System.Security.Claims;
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
/// Тестовый класс для AddUserExternalLoginCommandHandler
/// </summary>
public class AddUserExternalLoginCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика команды.
  /// </summary>
  private readonly AddUserExternalLoginCommandHandler _handler;

  /// <summary>
  /// Поле, представляющее текущего пользователя с его утверждениями (claims).
  /// </summary>
  private readonly ClaimsPrincipal _claimsPrincipal;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public AddUserExternalLoginCommandHandlerTest()
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

    _handler = new AddUserExternalLoginCommandHandler(_userManagerMock.Object);

    _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([
      new Claim(ClaimTypes.Email, "test@example.com")
    ]));
  }

  /// <summary>
  /// Проверка на случай, когда все данные валиды.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_AddsExternalLogin()
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

    _userManagerMock
      .Setup(m => m.AddLoginAsync(It.IsAny<AppUser>(), It.IsAny<ExternalLoginInfo>()))
      .ReturnsAsync(IdentityResult.Success);

    var command = new AddUserExternalLoginCommand
    {
      UserId = Guid.NewGuid(),
      LoginInfo = new ExternalLoginInfo(_claimsPrincipal, "TestProvider", "TestKey", "TestDisplayName")
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
  /// Проверка на случай, когда пользователя не существует.
  /// </summary>
  [Fact]
  public async Task Handle_WhenUserNotFoundById_ThrowsUserNotFoundException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => null);

    var command = new AddUserExternalLoginCommand
    {
      UserId = Guid.NewGuid(),
      LoginInfo = new ExternalLoginInfo(_claimsPrincipal, "TestProvider", "TestKey", "TestDisplayName")
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  /// Проверка на случай, когда провайдер уже существует.
  /// </summary>
  [Fact]
  public async Task Handle_WhenProviderAlreadyExists_ThrowsLoginAlreadyExistsException()
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

    var command = new AddUserExternalLoginCommand
    {
      UserId = Guid.NewGuid(),
      LoginInfo = new ExternalLoginInfo(_claimsPrincipal, "TestProvider", "TestKey", "TestDisplayName")
    };

    // Act & Assert
    await Assert.ThrowsAsync<LoginAlreadyExistsException>(() => _handler.Handle(command, CancellationToken.None));
  }

  /// <summary>
  ///  Проверка на случай, когда логин уже ассоциирован с пользователем.
  /// </summary>
  [Fact]
  public async Task Handle_WhenLoginAlreadyAssociated_ThrowsLoginAlreadyExistsException()
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

    _userManagerMock
      .Setup(m => m.AddLoginAsync(It.IsAny<AppUser>(), It.IsAny<ExternalLoginInfo>()))
      .ReturnsAsync(IdentityResult.Failed());

    var command = new AddUserExternalLoginCommand
    {
      UserId = Guid.NewGuid(),
      LoginInfo = new ExternalLoginInfo(_claimsPrincipal, "TestProvider", "TestKey", "TestDisplayName")
    };

    // Act & Assert
    await Assert.ThrowsAsync<LoginAlreadyAssociatedException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
