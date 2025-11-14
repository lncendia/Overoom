using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Emails;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using MassTransit;

namespace Identix.Application.Services.Commands.Email;

/// <summary>
/// Обработчик команды запроса изменения электронной почты пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="publishEndpoint">Сервис для публикации событий.</param>
public class RequestChangeEmailCommandHandler(UserManager<AppUser> userManager, IPublishEndpoint publishEndpoint)
    : IRequestHandler<RequestChangeEmailCommand>
{
    /// <summary>
    /// Метод обработки команды запроса изменения электронной почты пользователя.
    /// </summary>
    /// <param name="request">Запрос на изменение электронной почты.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
    /// <exception cref="InvalidPasswordException">Вызывается, если пароль указан неверно.</exception>
    public async Task Handle(RequestChangeEmailCommand request, CancellationToken cancellationToken)
    {
        // Поиск пользователя по идентификатору.
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        // Вызываем исключение если пользователь не найден
        if (user == null) throw new UserNotFoundException();

        // Проверяем есть ли хэш пароля у пользователя
        if (user.PasswordHash != null)
        {
            // Если нет пароля, то выкидваем исключение
            if (request.Password == null) throw new PasswordNeededException();

            // Если пароль указан неверно, то выкидываем исключение
            if (!await userManager.CheckPasswordAsync(user, request.Password)) throw new InvalidPasswordException();
        }
        
        // Генерация кода подтверждения для изменения электронной почты.
        var code = await userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);

        // Формирование URL для подтверждения изменения электронной почты.
        var url = user.GenerateMailConfirmUrl(request.ResetUrl, code, request.ReturnUrl, new KeyValuePair<string, object>("Email", request.NewEmail));
        
        // Отправка электронного письма со ссылкой для подтверждения изменения электронной почты.
        var message = new ConfirmMailChangeEmail { Recipient = request.NewEmail, ConfirmLink = url };
        await publishEndpoint.SkipOutbox().Publish(new SendEmail { Message = message }, cancellationToken);
    }
}