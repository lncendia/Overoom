using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.TwoFactor;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.TwoFactor;
using MassTransit;

namespace Identix.Tests.UnitTests.Commands.TwoFactor;

/// <summary>
/// Тестовый класс для RequestTwoFactorCodeEmailCommandHandler.
/// </summary>
public class RequestTwoFactorCodeEmailCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly RequestTwoFactorCodeEmailCommandHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public RequestTwoFactorCodeEmailCommandHandlerTest()
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

    _handler = new RequestTwoFactorCodeEmailCommandHandler(_userManagerMock.Object,
      new Mock<IBus>().Object);
  }

  /// <summary>
  /// Проверка валидной команды для отправки кода 2FA на почту .
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_SendRequest()
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
      .Setup(m => m.IsEmailConfirmedAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(() => true);

    var command = new RequestTwoFactorCodeEmailCommand { UserId = Guid.NewGuid() };

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

    var command = new RequestTwoFactorCodeEmailCommand { UserId = Guid.NewGuid() };

    // Act & Assert
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
