using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using Identix.Application.Services.Commands.Email;

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
    /// Поле Mock объекта BackgroundJobClientV2.
    /// </summary>
    private readonly Mock<IBackgroundJobClientV2> _backgroundJobServiceMock = new();

    /// <summary>
    /// Поле обработчика.
    /// </summary>
    private readonly RequestChangeEmailCommandHandler _handler;

    /// <summary>
    /// Конструктор.
    /// </summary>
    public RequestChangeEmailCommandHandlerTest()
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

        // Инициализация обработчика.
        _handler = new RequestChangeEmailCommandHandler(_userManagerMock.Object, _backgroundJobServiceMock.Object);
    }
    
    /// <summary>
    /// Проверка валидной команды запроса на смену электронной почты у пользователя.
    /// </summary>
    [Fact]
    public async Task Handle_ValidCommand_SendRequest()
    {
        // Arrange
        // Настройка mock объекта UserManager для возвращения пользователя при вызове FindByIdAsync.
        _userManagerMock
                
            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            
            // Возвращаем тестового пользователя.  
            .ReturnsAsync(() => new AppUser
        {
            UserName = "test",
            Email = "test@example.com",
            RegistrationTimeUtc = DateTime.UtcNow,
            LastAuthTimeUtc = DateTime.UtcNow,
            
        });

        // Настройка mock объекта UserManager для возврата true(почта подтверждена) при вызове IsEmailConfirmedAsync.
        _userManagerMock
                
            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.IsEmailConfirmedAsync(It.IsAny<AppUser>()))
            
            // Возвращаем true -> почта подтверждена.
            .ReturnsAsync(() => true);
        
        // Создаем команду для запроса изменения электронной почты.
        var command = new RequestChangeEmailCommand
        {
            // Задаем Id пользователя.
            UserId = Guid.NewGuid(),
            
            // Задаем новый электронную почту.
            NewEmail = "new_test@example.com",
           
            // Задаем url для сброса электронной почты.
            ResetUrl = "test_url"
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
    /// Проверка случая, когда пользователь не найден по Id.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserNotFoundById_ThrowsUserNotFoundException()
    {
        // Arrange
        // Настройка mock объекта UserManager для возвращения null при вызове FindByIdAsync.
        _userManagerMock
                
            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            
            // Возвращаем null.
            .ReturnsAsync(() => null);
        
        // Создаем команду для запроса изменения электронной почты.
        var command = new RequestChangeEmailCommand
        {
            // Задаем Id пользователя.
            UserId = Guid.NewGuid(),
            
            // Задаем новый электронную почту.
            NewEmail = "new_test@example.com",
           
            // Задаем url для сброса электронной почты.
            ResetUrl = "test_url"
        };
        
        // Act & Assert
        // Проверка, что выполнение метода Handle приводит к возникновению исключения UserNotFoundException.
        await Assert.ThrowsAsync<UserNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
    
    /// <summary>
    /// Проверка случая, когда почта не подтверждена у пользователя.
    /// </summary>
    [Fact]
    public async Task Handle_WhenEmailNotConfirmed_ThrowsEmailNotConfirmedException()
    {
        // Arrange
        // Настройка mock объекта UserManager для возвращения пользователя при вызове FindByIdAsync.
        _userManagerMock
                
            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            
            // Возвращаем тестового пользователя.  
            .ReturnsAsync(() => new AppUser
        {
            UserName = "test",
            Email = "test@example.com",
            RegistrationTimeUtc = DateTime.UtcNow,
            LastAuthTimeUtc = DateTime.UtcNow,
            
        });

        // Настройка mock объекта UserManager для возврата false(почта не подтверждена) при вызове IsEmailConfirmedAsync.
        _userManagerMock
                
            // Выбираем метод, к которому делаем заглушку.  
            .Setup(m => m.IsEmailConfirmedAsync(It.IsAny<AppUser>()))
            
            // Возвращаем false -> почта не подтверждена.
            .ReturnsAsync(() => false);
        
        // Создаем команду для запроса изменения электронной почты.
        var command = new RequestChangeEmailCommand
        {
            // Задаем Id пользователя.
            UserId = Guid.NewGuid(),
            
            // Задаем новый электронную почту.
            NewEmail = "new_test@example.com",
           
            // Задаем url для сброса электронной почты.
            ResetUrl = "test_url"
        };
        
        // Act & Assert
        // Проверка, что выполнение метода Handle приводит к возникновению исключения EmailNotConfirmedException.
        await Assert.ThrowsAsync<EmailNotConfirmedException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}