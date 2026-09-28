using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.TwoFactor;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.TwoFactor;

/// <summary>
/// Обработчик команды верификации подключения пользователю 2FA
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class VerifySetupTwoFactorTokenCommandHandler(UserManager<AppUser> userManager)
  : IRequestHandler<VerifySetupTwoFactorTokenCommand, IReadOnlyCollection<string>>
{
  /// <summary>
  /// Количество кодов восстановления для генерации
  /// </summary>
  private const int RecoveryCodesCount = 5;

  /// <summary>
  /// Метод верификации и подключения 2FA пользователю
  /// </summary>
  /// <param name="request">Запрос на подключение 2FA</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает резервные коды аутентификации</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не был найден</exception>
  /// <exception cref="InvalidCodeException">Вызывается, если предоставленный код не был верифицирован</exception>
  public async Task<IReadOnlyCollection<string>> Handle(VerifySetupTwoFactorTokenCommand request,
    CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();
    if (await userManager.GetTwoFactorEnabledAsync(user)) throw new TwoFactorAlreadyEnabledException();

    bool isValid = await userManager.VerifyTwoFactorTokenAsync(user,
      userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code);

    if (!isValid) throw new InvalidCodeException();

    await userManager.SetTwoFactorEnabledAsync(user, true);
    IEnumerable<string>? codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, RecoveryCodesCount);

    return codes?.ToArray() ?? [];
  }
}