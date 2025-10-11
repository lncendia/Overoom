using Common.Application.EmailService;
using Hangfire;
using Identix.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.TwoFactor;
using Identix.Application.Abstractions.Emails;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.TwoFactor;

/// <summary>
/// Обработчик команды отправки кода 2FA на почту
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="backgroundJobClient">Клиент для постановки фоновых задач.</param>
public class RequestTwoFactorCodeEmailCommandHandler(
    UserManager<AppUser> userManager,
    IBackgroundJobClientV2 backgroundJobClient)
    : IRequestHandler<RequestTwoFactorCodeEmailCommand>
{
    /// <summary>
    /// Название EmailTokenProvider'а
    /// </summary>
    private const string EmailTokenProvider = "Email";

    /// <summary>
    /// Метод обработки команды отправки кода 2FA на почту
    /// </summary>
    /// <param name="request">Запрос на отправку кода 2FA на почту</param>
    /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
    /// <exception cref="UserNotFoundException">Вызывается, если пользователь не был найден</exception>
    /// <exception cref="EmailNotConfirmedException">Вызывается, если почта пользователя не подтверждена.</exception>
    public async Task Handle(RequestTwoFactorCodeEmailCommand request, CancellationToken cancellationToken)
    {
        // Поиск пользователя по идентификатору
        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        // Вызываем исключение UserNotFoundException если не найден пользователь
        if (user == null) throw new UserNotFoundException();

        // Проверка, что почта подтверждена
        if (!await userManager.IsEmailConfirmedAsync(user)) throw new EmailNotConfirmedException();

        // Генерируем код 2fa
        var code = await userManager.GenerateTwoFactorTokenAsync(user, EmailTokenProvider);

        // Отправляем письмо
        backgroundJobClient.Enqueue<IEmailService>(Constants.Hangfire.Queue,
            service => service.SendAsync(new TwoFactorCodeEmail { Recipient = user.Email!, Code = code }, CancellationToken.None));
    }
}