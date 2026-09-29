using MediatR;
using Microsoft.AspNetCore.Identity;
using Identix.Application.Abstractions.Commands.Password;
using Identix.Application.Abstractions.Entities;
using Identix.Application.Abstractions.Exceptions;

namespace Identix.Application.Services.Commands.Password;

/// <summary>
/// Обработчик для выполнения команды сброса пароля
/// </summary>
/// <param name="userManager">Менеджер пользователей, предоставленный ASP.NET Core Identity.</param>
public class RecoverPasswordCommandHandler(UserManager<AppUser> userManager) : IRequestHandler<RecoverPasswordCommand>
{
  /// <summary>
  /// Обработка команды RecoverPasswordCommand для сброса пароля.
  /// </summary>
  /// <param name="request">Запрос на сброс пароля пользователя.</param>
  /// <param name="cancellationToken">Токен отмены для асинхронной операции.</param>
  /// <exception cref="UserNotFoundException">Вызывается, если пользователь не найден.</exception>
  /// <exception cref="InvalidCodeException">Вызывается, если введенный код неверен.</exception>
  /// <exception cref="PasswordValidationException">Вызывается, если валидация пароля не прошла.</exception>
  public async Task Handle(RecoverPasswordCommand request, CancellationToken cancellationToken)
  {
    AppUser? user = await userManager.FindByIdAsync(request.UserId.ToString());
    if (user == null) throw new UserNotFoundException();

    IdentityResult result = await userManager.ResetPasswordAsync(user, request.Code, request.NewPassword);

    if (!result.Succeeded)
    {
      if (result.Errors.Any(e => e.Code == "InvalidToken")) throw new InvalidCodeException();

      var passwordValidationErrors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
      throw new PasswordValidationException { ValidationErrors = passwordValidationErrors };
    }
  }
}