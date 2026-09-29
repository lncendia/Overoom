using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.Password;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.Password;
using MassTransit;

namespace Identix.Tests.UnitTests.Commands.Password;

/// <summary>
/// Тестовый класс для RequestRecoverPasswordCommandHandler.
/// </summary>
public class RequestRecoverPasswordCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly RequestRecoverPasswordCommandHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public RequestRecoverPasswordCommandHandlerTest()
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

    _handler = new RequestRecoverPasswordCommandHandler(_userManagerMock.Object, new Mock<IBus>().Object);
  }

  /// <summary>
  /// Проверка валидной команды запроса на восстановление пароля у пользователя.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_SendRequest()
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
      .Setup(m => m.IsEmailConfirmedAsync(It.IsAny<AppUser>()))
      .ReturnsAsync(() => true);

    var command = new RequestRecoverPasswordCommand
    {
      Email = "test@example.com",
      ResetUrl = "test_url"
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

    var command = new RequestRecoverPasswordCommand
    {
      Email = "test@example.com",
      ResetUrl = "test_url"
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
