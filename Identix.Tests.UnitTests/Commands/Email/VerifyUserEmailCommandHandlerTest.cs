using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.Email;
using MassTransit;
using MassTransit.MongoDbIntegration;

namespace Identix.Tests.UnitTests.Commands.Email;

/// <summary>
/// Тестовый класс для VerifyUserEmailCommandHandler.
/// </summary>
public class VerifyUserEmailCommandHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly VerifyEmailCommandHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public VerifyUserEmailCommandHandlerTest()
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

    _handler = new VerifyEmailCommandHandler(_userManagerMock.Object, new Mock<IPublishEndpoint>().Object,
      new Mock<MongoDbContext>().Object);
  }

  /// <summary>
  /// Проверка валидной команды подтверждения электронной почты пользователя.
  /// </summary>
  [Fact]
  public async Task Handle_ValidCommand_ConfirmEmail()
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
      .Setup(m => m.ConfirmEmailAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Success);

    _userManagerMock
      .Setup(m => m.GetClaimsAsync(It.IsAny<AppUser>()))
      .ReturnsAsync([]);

    var command = new VerifyEmailCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code"
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

    var command = new VerifyEmailCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code"
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
      .Setup(m => m.ConfirmEmailAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
      .ReturnsAsync(IdentityResult.Failed());

    var command = new VerifyEmailCommand
    {
      UserId = Guid.NewGuid(),
      Code = "test_code"
    };

    // Act & Assert
    await Assert.ThrowsAsync<InvalidCodeException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
