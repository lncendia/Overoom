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
    AppUser? user = await userManager.FindByEmailAsync(request.Email);
    if (user == null) throw new UserNotFoundException();

    string code = await userManager.GeneratePasswordResetTokenAsync(user);
    string url = user.GenerateMailConfirmUrl(request.ResetUrl, code, request.ReturnUrl);
    var message = new ConfirmRecoverPasswordEmail { Recipient = request.Email, ConfirmLink = url };
    await publishEndpoint.SkipOutbox().Publish(new SendEmail { Message = message }, cancellationToken);
  }
}