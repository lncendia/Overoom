using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Password;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.Password;

/// <summary>
/// Обработчик для выполнения команды смены пароля
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class ChangePasswordCommandHandler(UserManager<AppUser> userManager)
  : IRequestHandler<ChangePasswordCommand, AppUser>
{
  /// <summary>
  /// Обработка команды ChangePasswordCommand, обновляя пароль пользователя.
  /// </summary>
  /// <param name="request">Запрос на смену пароля пользователя.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <returns>Возвращает обновленного пользователя.</returns>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  /// <exception cref="PasswordNeededException">Вызывается, если не введен старый пароль.</exception>
  /// <exception cref="PasswordValidationException">Вызывается, если валидация пароля не прошла.</exception>
  public async Task<AppUser> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();

    IdentityResult result;

    if (user.PasswordHash == null)
    {
      result = await userManager.AddPasswordAsync(user, request.NewPassword);
    }
    else
    {
      if (request.OldPassword == null) throw new PasswordNeededException();

      result = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
    }

    if (!result.Succeeded)
    {
      var passwordValidationErrors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
      throw new PasswordValidationException { ValidationErrors = passwordValidationErrors };
    }

    return user;
  }
}