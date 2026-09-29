using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.Email;
using MassTransit;

namespace Identix.Tests.UnitTests.Commands.Email;

/// <summary>
/// Тестовый класс для RequestChangeEmailCommandHandler.
/// </summary>
public class RequestChangeEmailCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly RequestChangeEmailCommandHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public RequestChangeEmailCommandHandlerTest()
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

    _handler = new RequestChangeEmailCommandHandler(_userManagerMock.Object, new Mock<IBus>().Object);
  }

  /// <summary>
  /// Проверка валидной команды запроса на смену электронной почты у пользователя.
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

    var command = new RequestChangeEmailCommand
    {
      UserId = Guid.NewGuid(),
      NewEmail = "new_test@example.com",
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
  /// Проверка случая, когда пользователь не найден по Id.
  /// </summary>
  [Fact]
  public async Task Handle_WhenUserNotFoundById_ThrowsUserNotFoundException()
  {
    // Arrange
    _userManagerMock
      .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
      .ReturnsAsync(() => null);

    var command = new RequestChangeEmailCommand
    {
      UserId = Guid.NewGuid(),
      NewEmail = "new_test@example.com",
      ResetUrl = "test_url"
    };

    // Act & Assert
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
