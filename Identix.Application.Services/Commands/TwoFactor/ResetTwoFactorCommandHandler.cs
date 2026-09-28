using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.TwoFactor;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Enums;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.TwoFactor;

/// <summary>
/// Обработчик команды сброса 2фа
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class ResetTwoFactorCommandHandler(UserManager<AppUser> userManager)
  : IRequestHandler<ResetTwoFactorCommand, AppUser>
{
  /// <summary>
  /// Название EmailTokenProvider'а
  /// </summary>
  private const string EmailTokenProvider = "Email";

  /// <summary>
  /// Метод обработки команды сброса 2фа
  /// </summary>
  /// <param name="request">Запрос на сброс 2фа</param>
  ///<param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает пользователя со сброшенной 2фа</returns>
  /// <exception cref="UserNotFoundException">Возникает, если пользователь не был найден</exception>
  /// <exception cref="ArgumentOutOfRangeException">Возникает, при неопознанном типе кода сброса 2фа</exception>
  /// <exception cref="InvalidCodeException">Возникает, при невалидном коде</exception>
  /// <exception cref="TwoFactorNotEnabledException">Возникает, при попытка отключить 2фа, когда она уже отключена</exception>
  public async Task<AppUser> Handle(ResetTwoFactorCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();
    if (!await userManager.GetTwoFactorEnabledAsync(user)) throw new TwoFactorNotEnabledException();

    bool result = request.Type switch
    {
      CodeType.Authenticator => await userManager.VerifyTwoFactorTokenAsync(user,
        userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code),

      CodeType.Email => await userManager.VerifyTwoFactorTokenAsync(user, EmailTokenProvider, request.Code),

      CodeType.RecoveryCode => (await userManager.RedeemTwoFactorRecoveryCodeAsync(user, request.Code)).Succeeded,

      _ => throw new ArgumentOutOfRangeException(nameof(request))
    };

    if (!result) throw new InvalidCodeException();

    await userManager.SetTwoFactorEnabledAsync(user, false);
    await userManager.ResetAuthenticatorKeyAsync(user);

    return user;
  }
}