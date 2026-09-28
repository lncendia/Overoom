using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Enums;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.Authentication;

/// <summary>
/// Обработчик команды прохождения 2FA
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class AuthenticateTwoFactorCommandHandler(UserManager<AppUser> userManager)
  : IRequestHandler<AuthenticateTwoFactorCommand, AppUser>
{
  /// <summary>
  /// Название EmailTokenProvider'а
  /// </summary>
  private const string EmailTokenProvider = "Email";

  /// <summary>
  /// Метод обработки команды прохождения 2FA
  /// </summary>
  /// <param name="request">Запрос на прохождение 2FA</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает аутентифицированного пользователя</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не был найден</exception>
  /// <exception cref="InvalidCodeException">Вызывается, если код аутентификации не верен</exception>
  /// <exception cref="ArgumentOutOfRangeException">Возникает, при неопознанном типе кода сброса 2фа</exception>
  public async Task<AppUser> Handle(AuthenticateTwoFactorCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();

    bool result = request.Type switch
    {
      CodeType.Authenticator => await userManager.VerifyTwoFactorTokenAsync(user,
        userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code),

      CodeType.Email => await userManager.VerifyTwoFactorTokenAsync(user, EmailTokenProvider, request.Code),

      CodeType.RecoveryCode => (await userManager.RedeemTwoFactorRecoveryCodeAsync(user, request.Code)).Succeeded,

      _ => throw new ArgumentOutOfRangeException(nameof(request))
    };

    if (!result) throw new InvalidCodeException();

    user.LastAuthTimeUtc = DateTime.UtcNow;
    await userManager.UpdateAsync(user);

    return user;
  }
}