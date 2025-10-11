using Common.Application.EmailService;
using Hangfire;
using Identix.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Email;
using Identix.Application.Abstractions.Emails;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.Email;

/// <summary>
/// Обработчик команды запроса изменения электронной почты пользователя.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="backgroundJobClient">Клиент для постановки фоновых задач.</param>
public class RequestChangeEmailCommandHandler(
    UserManager<AppUser> userManager,
    IBackgroundJobClientV2 backgroundJobClient)
    : IRequestHandler<RequestChangeEmailCommand>
{
    /// <summary>
    /// Метод обработки команды запроса изменения электронной почты пользователя.
    /// </summary>
    /// <param name="request">Запрос на изменение электронной почты.</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
    /// <exception cref="EmailNotConfirmedException">Вызывается, если почта пользователя не подтверждена.</exception>
    public async Task Handle(RequestChangeEmailCommand request, CancellationToken cancellationToken)
    {
        // Поиск пользователя по идентификатору.
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        // Вызываем исключение если пользователь не найден
        if (user == null) throw new UserNotFoundException();

        // Проверка, что почта подтверждена
        if (!await userManager.IsEmailConfirmedAsync(user)) throw new EmailNotConfirmedException();

        // Генерация кода подтверждения для изменения электронной почты.
        var code = await userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);

        // Формирование URL для подтверждения изменения электронной почты.
        var url = GenerateMailUrl(request.ResetUrl, request.NewEmail, code);

        // Отправка электронного письма со ссылкой для подтверждения изменения электронной почты.
        backgroundJobClient.Enqueue<IEmailService>(Constants.Hangfire.Queue,
            service => service.SendAsync(new ConfirmMailChangeEmail { Recipient = user.Email!, ConfirmLink = url }, CancellationToken.None));
    }

    /// <summary>
    /// Генерирует URL для подтверждения изменения электронной почты.
    /// </summary>
    /// <param name="url">Базовый URL.</param>
    /// <param name="email">Новый адрес электронной почты.</param>
    /// <param name="code">Код подтверждения.</param>
    /// <returns>Сгенерированный URL.</returns>
    private static string GenerateMailUrl(string url, string email, string code)
    {
        // Создаем объект UriBuilder с базовым URL
        var uriBuilder = new UriBuilder(url);

        // Получаем коллекцию параметров запроса
        var queryParameters = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

        // Добавляем параметр "email" со значением
        queryParameters["email"] = email;

        // Добавляем параметр "code" со значением
        queryParameters["code"] = code;

        // Устанавливаем обновленную строку запроса
        uriBuilder.Query = queryParameters.ToString();

        // Получаем обновленный URL
        return uriBuilder.ToString();
    }
}