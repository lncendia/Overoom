using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Abstractions.Queries;
using Identix.Application.Services.Queries;

namespace Identix.Tests.UnitTests.Queries;

/// <summary>
/// Тестовый класс для UserLoginsQueryHandler.
/// </summary>
public class UserLoginsQueryHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly UserLoginsQueryHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public UserLoginsQueryHandlerTest()
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

    _handler = new UserLoginsQueryHandler(_userManagerMock.Object);
  }

  /// <summary>
  /// Проверка валидного запроса на получение списка внешних учетных записей пользователя.
  /// </summary>
  [Fact]
  public async Task Handle_ValidQuery_GetLogins()
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

    var command = new UserLoginsQuery { Id = Guid.NewGuid() };

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

    var command = new UserLoginsQuery { Id = Guid.NewGuid() };

    // Act & Assert
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
