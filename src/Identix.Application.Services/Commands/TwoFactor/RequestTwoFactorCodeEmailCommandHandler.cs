using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.TwoFactor;
using Identix.Application.Abstractions.Emails;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;
using MassTransit;

namespace Identix.Application.Services.Commands.TwoFactor;

/// <summary>
/// Обработчик команды отправки кода 2FA на почту
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
/// <param name="bus">Шина для отправки писем напрямую, минуя outbox</param>
public class RequestTwoFactorCodeEmailCommandHandler(UserManager<AppUser> userManager, IBus bus)
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
  public async Task Handle(RequestTwoFactorCodeEmailCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();

    string code = await userManager.GenerateTwoFactorTokenAsync(user, EmailTokenProvider);
    var message = new TwoFactorCodeEmail { Recipient = user.Email!, Code = code };
    await bus.Publish(new SendEmail { Message = message }, cancellationToken);
  }
}