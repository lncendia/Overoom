using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.TwoFactor;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.TwoFactor;

/// <summary>
/// Обработчик команды получения аутентификатора пользователем для подключения 2FA
/// </summary>
/// <param name="userManager"></param>
public class SetupTwoFactorCommandHandler(UserManager<AppUser> userManager)
  : IRequestHandler<SetupTwoFactorCommand, (AppUser, string)>
{
  /// <summary>
  /// Метод установки аутентификатора пользователя для подключения 2FA
  /// </summary>
  /// <param name="request">Запрос на установку аутентификатора</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Кортеж, содержащий пользователя и код установленного аутентификатора</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не был найден</exception>
  /// <exception cref="TwoFactorAlreadyEnabledException">Вызывается, если 2FA уже подключена, и аутентификатор уже был установлен</exception>
  public async Task<(AppUser, string)> Handle(SetupTwoFactorCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();
    if (await userManager.GetTwoFactorEnabledAsync(user)) throw new TwoFactorAlreadyEnabledException();

    string? authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);
    if (authenticatorKey != null) return (user, authenticatorKey);

    await userManager.ResetAuthenticatorKeyAsync(user);
    authenticatorKey = await userManager.GetAuthenticatorKeyAsync(user);

    return (user, authenticatorKey!);
  }
}