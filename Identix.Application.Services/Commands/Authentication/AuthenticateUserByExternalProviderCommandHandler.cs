using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Authentication;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.Authentication;

/// <summary>
/// Класс обработчика команды аутентификации пользователя через внешний провайдер.
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class AuthenticateUserByExternalProviderCommandHandler(UserManager<AppUser> userManager)
  : IRequestHandler<AuthenticateUserByExternalProviderCommand, AppUser>
{
  /// <summary>
  /// Обработка команды аутентификации пользователя через внешний провайдер.
  /// </summary>
  /// <param name="request">Запрос на аутентификацию пользователя через внешний провайдер.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Объект пользователя в случае успешной аутентификации.</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  /// <exception cref="UserLockoutException">Вызывается, если пользователь заблокирован.</exception>
  public async Task<AppUser> Handle(AuthenticateUserByExternalProviderCommand request,
    CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByLoginAsync(request.LoginProvider, request.ProviderKey);
    if (user == null) throw new UserNotFoundException();

    if (await userManager.IsLockedOutAsync(user))
    {
      throw new UserLockoutException();
    }

    await userManager.ResetAccessFailedCountAsync(user);
    bool is2FaEnabled = await userManager.GetTwoFactorEnabledAsync(user);
    if (is2FaEnabled) throw new TwoFactorRequiredException(user);

    user.LastAuthTimeUtc = DateTime.UtcNow;
    await userManager.UpdateAsync(user);

    return user;
  }
}