using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Emails;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using MassTransit;

namespace Identix.Application.Services.Commands.Authentication;

/// <summary>
/// Класс обработчика команды аутентификации пользователя по паролю.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="publishEndpoint">Сервис для публикации интеграционных событий.</param>
public class AuthenticateUserByPasswordCommandHandler(UserManager<AppUser> userManager, IPublishEndpoint publishEndpoint) : IRequestHandler<AuthenticateUserByPasswordCommand, AppUser>
{
    /// <summary>
    /// Обработка команды аутентификации пользователя по паролю.
    /// </summary>
    /// <param name="request">Запрос на аутентификацию пользователя.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <returns>Объект пользователя в случае успешной аутентификации.</returns>
    /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
    /// <exception cref="UserLockoutException">Вызывается, если пользователь заблокирован.</exception>
    /// <exception cref="InvalidPasswordException">Вызывается, если валидация пароля не прошла.</exception>
    /// <exception cref="TwoFactorRequiredException">Вызывается, если у пользователя включена 2фа.</exception>
    public async Task<AppUser> Handle(AuthenticateUserByPasswordCommand request, CancellationToken cancellationToken)
    {
        // Получаем пользователя по его электронной почте.
        var user = await userManager.FindByEmailAsync(request.Email);
        
        // Если пользователь не найден, вызываем исключение UserNotFoundException.
        if (user == null) throw new UserNotFoundException();
        
        // Проверяем заблокирован ли пользователь 
        if (await userManager.IsLockedOutAsync(user))
        {
            // Если пользователь заблокирован, вызываем исключение UserLockoutException.
            throw new UserLockoutException();
        }
        
        // Проверяем правильность введенного пароля.
        var success = await userManager.CheckPasswordAsync(user, request.Password);

        // Если пароль верный
        if (success)
        {
            // Сбрасываем счетчик неудачных попыток входа
            await userManager.ResetAccessFailedCountAsync(user);
            
            // Проверяем, подтверждена ли почта у пользователя
            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                // Генерация кода подтверждения и формирование URL для подтверждения электронной почты.
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

                // Генерация URL подтверждения почты
                var url = user.GenerateMailConfirmUrl(request.ConfirmUrl, code, request.ReturnUrl);

                // Отправка электронного письма со ссылкой для подтверждения регистрации.
                var message = new ConfirmRegistrationEmail { Recipient = user.Email!, ConfirmLink = url };
                await publishEndpoint.SkipOutbox().Publish(new SendEmail { Message = message }, cancellationToken);
                
                // Вызываем исключение
                throw new EmailNotConfirmedException();
            }
            
            // Проверяем, включена ли 2фа у пользователя
            var is2FaEnabled = await userManager.GetTwoFactorEnabledAsync(user);

            // Если у пользователя включена 2фа выбрасываем исключение
            if (is2FaEnabled) throw new TwoFactorRequiredException(user);
            
            // Устанавливаем время последнего входа
            user.LastAuthTimeUtc = DateTime.UtcNow;

            // Обновляем данные
            await userManager.UpdateAsync(user);
            
            // Возвращаем пользователя
            return user;
        }
        
        // Если пароль неверный.
        // Инкриминируем счетчик неудачных попыток
        await userManager.AccessFailedAsync(user);
        
        // Вызываем исключение InvalidPasswordException.
        throw new InvalidPasswordException();
    }
}