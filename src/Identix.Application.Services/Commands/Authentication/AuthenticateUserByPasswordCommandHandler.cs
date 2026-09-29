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
/// <param name="bus">Шина для отправки писем напрямую, минуя outbox</param>
public class AuthenticateUserByPasswordCommandHandler(
  UserManager<AppUser> userManager,
  IBus bus) : IRequestHandler<AuthenticateUserByPasswordCommand, AppUser>
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
    AppUser? user = await userManager.FindByEmailAsync(request.Email);
    if (user == null) throw new UserNotFoundException();

    if (await userManager.IsLockedOutAsync(user))
    {
      throw new UserLockoutException();
    }

    bool success = await userManager.CheckPasswordAsync(user, request.Password);

    if (success)
    {
      await userManager.ResetAccessFailedCountAsync(user);

      if (!await userManager.IsEmailConfirmedAsync(user))
      {
        string code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        string url = user.GenerateMailConfirmUrl(request.ConfirmUrl, code, request.ReturnUrl);
        var message = new ConfirmRegistrationEmail { Recipient = user.Email!, ConfirmLink = url };
        await bus.Publish(new SendEmail { Message = message }, cancellationToken);
        throw new EmailNotConfirmedException();
      }

      bool is2FaEnabled = await userManager.GetTwoFactorEnabledAsync(user);
      if (is2FaEnabled) throw new TwoFactorRequiredException(user);

      user.LastAuthTimeUtc = DateTime.UtcNow;
      await userManager.UpdateAsync(user);

      return user;
    }

    await userManager.AccessFailedAsync(user);
    throw new InvalidPasswordException();
  }
}