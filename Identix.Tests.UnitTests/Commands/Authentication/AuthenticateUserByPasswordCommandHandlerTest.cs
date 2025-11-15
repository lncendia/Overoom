using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.Authentication;
using MassTransit;

namespace Identix.Tests.UnitTests.Commands.Authentication;

/// <summary>
/// Тестовый класс для AuthenticateUserByPasswordCommandHandler
/// </summary>
public class AuthenticateUserByPasswordCommandHandlerTest
{
    /// <summary>
    /// Поле Mock объекта UserManager.
    /// </summary>
    private readonly Mock<UserManager<AppUser>> _userManagerMock;

    /// <summary>
    /// Поле обработчика.
    /// </summary>
    private readonly AuthenticateUserByPasswordCommandHandler _handler;

    /// <summary>
    /// Конструктор.
    /// </summary>
    public AuthenticateUserByPasswordCommandHandlerTest()
    {
        // Инициализация mock объекта UserManager.
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

        // Инициализация обработчика.
        _handler = new AuthenticateUserByPasswordCommandHandler(_userManagerMock.Object, publishEndpointMock.Object);
    }

    /// <summary>
    /// Проверка валидной команды на аутентификацию по паролю.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_AuthenticateByPassword()
    {
        // Arrange
        // Настройка mock объекта UserManager для возвращения пользователя при вызове FindByEmailAsync.
        _userManagerMock

            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))

            // Возвращаем тестового пользователя.  
            .ReturnsAsync(() => new AppUser
            {
                UserName = "test",
                Email = "test@example.com",
                RegistrationTimeUtc = DateTime.UtcNow,
                LastAuthTimeUtc = DateTime.UtcNow
            });

        // Настройка mock объекта UserManager для возвращения false при вызове IsLockedOutAsync.
        _userManagerMock
            .Setup(m => m.IsLockedOutAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(() => false);

        _userManagerMock
            .Setup(m => m.IsEmailConfirmedAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(() => true);

        // Настройка mock объекта UserManager для возвращения true при вызове CheckPasswordAsync.
        _userManagerMock

            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()))

            // Возвращаем false.
            .ReturnsAsync(() => true);


        // Создаем команду для аутентификации пользователя по паролю.
        var command = new AuthenticateUserByPasswordCommand
        {
            Email = "test@example.com",
            Password = "password",
            ConfirmUrl = "https://google.com"
        };

        // Act
        // Вызов обработчика команды и ожидание возникновения исключения (если такое есть).
        var exception = await Record.ExceptionAsync(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });

        // Assert
        // Проверка на отсутствие исключения.
        Assert.Null(exception);
    }

    /// <summary>
    /// Проверка случая, когда пользователь не найден по почте.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserNotFoundByEmail_ThrowsUserNotFoundException()
    {
        // Arrange
        // Настройка mock объекта UserManager для возвращения null при вызове FindByEmailAsync.
        _userManagerMock

            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))

            // Возвращаем null
            .ReturnsAsync(() => null);

        // Создаем команду для аутентификации пользователя по паролю.
        var command = new AuthenticateUserByPasswordCommand
        {
            Email = "test@example.com",
            Password = "password",
            ConfirmUrl = "https://google.com"
        };

        // Act & Assert
        // Проверка, что выполнение метода Handle приводит к возникновению исключения UserNotFoundException.
        await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    /// <summary>
    /// Проверка случая, когда пользователь заблокирован.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserIsLockout_ThrowsUserLockoutException()
    {
        // Arrange
        // Настройка mock объекта UserManager для возвращения пользователя при вызове FindByEmailAsync.
        _userManagerMock

            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))

            // Возвращаем тестового пользователя.  
            .ReturnsAsync(() => new AppUser
            {
                UserName = "test",
                Email = "test@example.com",
                RegistrationTimeUtc = DateTime.UtcNow,
                LastAuthTimeUtc = DateTime.UtcNow
            });

        // Настройка mock объекта UserManager для возвращения true при вызове IsLockedOutAsync.
        _userManagerMock

            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.IsLockedOutAsync(It.IsAny<AppUser>()))

            // Возвращаем true, так как пользователь должен быть временно заблокирован. 
            .ReturnsAsync(() => true);

        // Создаем команду для аутентификации пользователя по паролю.
        var command = new AuthenticateUserByPasswordCommand
        {
            Email = "test@example.com",
            Password = "password",
            ConfirmUrl = "https://google.com"
        };

        // Act & Assert
        // Проверка, что выполнение метода Handle приводит к возникновению исключения UserLockoutException.
        await Assert.ThrowsAsync<UserLockoutException>(() => _handler.Handle(command, CancellationToken.None));
    }

    /// <summary>
    /// Проверка случая, когда пользователь ввел неверный пароль.
    /// </summary>
    [Fact]
    public async Task Handle_WhenWrongPassword_ThrowsInvalidPasswordException()
    {
        // Arrange
        // Настройка mock объекта UserManager для возвращения пользователя при вызове FindByEmailAsync.
        _userManagerMock

            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))

            // Возвращаем тестового пользователя.  
            .ReturnsAsync(() => new AppUser
            {
                UserName = "test",
                Email = "test@example.com",
                RegistrationTimeUtc = DateTime.UtcNow,
                LastAuthTimeUtc = DateTime.UtcNow
            });

        // Настройка mock объекта UserManager для возвращения false при вызове IsLockedOutAsync.
        _userManagerMock

            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.IsLockedOutAsync(It.IsAny<AppUser>()))

            // Возвращаем true, так как пользователь должен быть временно заблокирован. 
            .ReturnsAsync(() => false);

        // Настройка mock объекта UserManager для возвращения false при вызове CheckPasswordAsync.
        _userManagerMock

            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>()))

            // Возвращаем false.
            .ReturnsAsync(() => false);

        // Создаем команду для аутентификации пользователя по паролю.
        var command = new AuthenticateUserByPasswordCommand
        {
            Email = "test@example.com",
            Password = "password",
            ConfirmUrl = "https://google.com"
        };

        // Act & Assert
        // Проверка, что выполнение метода Handle приводит к возникновению исключения InvalidPasswordException.
        await Assert.ThrowsAsync<InvalidPasswordException>(() => _handler.Handle(command, CancellationToken.None));
    }
}