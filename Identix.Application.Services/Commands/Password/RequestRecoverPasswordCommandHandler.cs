using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Password;
using Identix.Application.Abstractions.Emails;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using MassTransit;

namespace Identix.Application.Services.Commands.Password;

/// <summary>
/// Обработчик команды запроса восстановления пароля пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="publishEndpoint">Сервис для публикации событий.</param>
public class RequestRecoverPasswordCommandHandler(UserManager<AppUser> userManager, IPublishEndpoint publishEndpoint)
    : IRequestHandler<RequestRecoverPasswordCommand>
{
    /// <summary>
    /// Метод обработки команды запроса восстановления пароля пользователя.
    /// </summary>
    /// <param name="request">Запрос на восстановление пароля.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
    public async Task Handle(RequestRecoverPasswordCommand request, CancellationToken cancellationToken)
    {
        // Поиск пользователя по адресу электронной почты.
        var user = await userManager.FindByEmailAsync(request.Email);

        // Вызываем исключение если пользователь не найден
        if (user == null) throw new UserNotFoundException();
        
        // Генерация кода сброса пароля.
        var code = await userManager.GeneratePasswordResetTokenAsync(user);

        // Формирование URL для подтверждения сброса пароля.
        var url = user.GenerateMailConfirmUrl(request.ResetUrl, code, request.ReturnUrl);

        // Отправка электронного письма со ссылкой для подтверждения сброса пароля.
        var message = new ConfirmRecoverPasswordEmail { Recipient = request.Email, ConfirmLink = url };
        await publishEndpoint.SkipOutbox().Publish(new SendEmail { Message = message }, cancellationToken);
    }
}