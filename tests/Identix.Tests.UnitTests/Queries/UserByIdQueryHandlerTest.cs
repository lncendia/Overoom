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
/// Тестовый класс для UserByIdQueryHandler.
/// </summary>
public class UserByIdQueryHandlerTest
{
  /// <summary>
  /// Поле Mock объекта UserManager.
  /// </summary>
  private readonly Mock<UserManager<AppUser>> _userManagerMock;

  /// <summary>
  /// Поле обработчика.
  /// </summary>
  private readonly UserByIdQueryHandler _handler;

  /// <summary>
  /// Конструктор.
  /// </summary>
  public UserByIdQueryHandlerTest()
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

    _handler = new UserByIdQueryHandler(_userManagerMock.Object);
  }

  /// <summary>
  /// Проверка валидного запроса на получение пользователя по идентификатору.
  /// </summary>
  [Fact]
  public async Task Handle_ValidQuery_GetUser()
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

    var command = new UserByIdQuery { Id = Guid.NewGuid() };

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

    var command = new UserByIdQuery { Id = Guid.NewGuid() };

    // Act & Assert
    await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
  }
}
